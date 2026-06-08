using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

/// <summary>
/// 퀵매치 대기실 팝업
/// - 매칭 타이머, 현재 플레이어 수, 방장 전용 시작 버튼 표시
/// </summary>
public class UI_QuickMatchPopup : UI_Popup
{
    #region Enum
    enum GameObjects
    {
        ContentObject,
        HostObject,     // 방장에게만 보이는 영역
        BtnStart,       // 방장 전용 시작 버튼
    }

    enum Texts
    {
        TitleText,
        TimeText,
        PlayerCountText,
        MinMaxText,
        MatchingStatusText,
    }
    #endregion

    // ─── 데이터 ───────────────────────────────────────────────────────────
    private int _minPlayerCount;
    private int _maxPlayerCount;
    private int _currentPlayerCount;
    private bool _isRoomMaster;

    private float _matchingTime;
    private string _titleBaseText;
    private CancellationTokenSource _cts;

    private static readonly string[] Dots = { ".", "..", "..." };

    // ─── 초기화 ───────────────────────────────────────────────────────────

    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));

        _titleBaseText = GetText((int)Texts.TitleText) != null
            ? GetText((int)Texts.TitleText).text
            : "매칭 중";

        return true;
    }

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }

    // ─── 외부 API ─────────────────────────────────────────────────────────

    /// <summary>팝업을 열고 초기 데이터를 설정합니다.</summary>
    public void SetInfo(int minPlayerCount, int maxPlayerCount, int currentPlayerCount, bool isRoomMaster)
    {
        _minPlayerCount = minPlayerCount;
        _maxPlayerCount = maxPlayerCount;
        _currentPlayerCount = currentPlayerCount;
        _isRoomMaster = isRoomMaster;

        _matchingTime = 0f;

        // 타이머 시작
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        StartMatchingTimer(_cts.Token).Forget();

        // 최소/최대 인원 텍스트
        GetText((int)Texts.MinMaxText)?.SetText($"{_minPlayerCount} ~ {_maxPlayerCount}명");

        Refresh();
    }

    /// <summary>플레이어 수만 갱신합니다.</summary>
    public void RefreshPlayerCount(int currentPlayerCount)
    {
        _currentPlayerCount = currentPlayerCount;
        Refresh();
    }

    /// <summary>방장 여부만 갱신합니다.</summary>
    public void RefreshMaster(bool isRoomMaster)
    {
        _isRoomMaster = isRoomMaster;
        Refresh();
    }

    // ─── 버튼 이벤트 ──────────────────────────────────────────────────────

    public void OnClickStart()
    {
        LobbyMatchMakingManager.Instance.StartQuickMatchGame();
    }

    public void OnClickCancel()
    {
        LobbyMatchMakingManager.Instance.CancelQuickMatch();
    }

    // ─── 내부 ─────────────────────────────────────────────────────────────

    private void Refresh()
    {
        // 플레이어 수
        GetText((int)Texts.PlayerCountText)?.SetText($"{_currentPlayerCount} / {_maxPlayerCount}");

        // 방장 전용 오브젝트
        GetObject((int)GameObjects.HostObject)?.SetActive(_isRoomMaster);

        // 매칭 상태 텍스트
        string status = _isRoomMaster
            ? $"최소 {_minPlayerCount}명 이상이면 시작 가능"
            : "매칭 대기 중...";
        GetText((int)Texts.MatchingStatusText)?.SetText(status);

        // 시작 버튼 (방장 + 최소 인원 이상)
        bool showStart = _isRoomMaster && _currentPlayerCount >= _minPlayerCount;
        GetObject((int)GameObjects.BtnStart)?.SetActive(showStart);
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async UniTaskVoid StartMatchingTimer(CancellationToken token)
    {
        try
        {
            float dotTimer = 0f;
            int dotIndex = 0;

            while (!token.IsCancellationRequested)
            {
                _matchingTime += Time.deltaTime;
                dotTimer += Time.deltaTime;

                GetText((int)Texts.TimeText)?.SetText(((int)_matchingTime).ToString());

                if (dotTimer >= 0.5f)
                {
                    dotTimer = 0f;
                    dotIndex = (dotIndex + 1) % Dots.Length;
                    GetText((int)Texts.TitleText)?.SetText(_titleBaseText + Dots[dotIndex]);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException) { }
    }
}
