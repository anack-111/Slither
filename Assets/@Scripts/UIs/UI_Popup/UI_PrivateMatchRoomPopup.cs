using TMPro;
using UnityEngine;

/// <summary>
/// 프라이빗 방 대기실 팝업
/// - 방 코드 표시, 현재 플레이어 수, 방장 전용 게임 시작 버튼
/// </summary>
public class UI_PrivateMatchRoomPopup : UI_Popup
{
    #region Enum
    enum GameObjects
    {
        ContentObject,
        BtnStart,       // 방장 전용 시작 버튼
        BtnCancel,
    }

    enum Texts
    {
        RoomCodeText,
        PlayerCountText,
    }
    #endregion

    // ─── 데이터 ───────────────────────────────────────────────────────────
    private string _roomCode;
    private int _maxPlayerCount;
    private int _currentPlayerCount;
    private bool _isRoomMaster;

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

        GetObject((int)GameObjects.BtnStart).BindEvent(OnClickStart);
        GetObject((int)GameObjects.BtnCancel).BindEvent(OnClickCancel);

        return true;
    }

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }

    // ─── 외부 API ─────────────────────────────────────────────────────────

    /// <summary>팝업을 열고 초기 데이터를 설정합니다.</summary>
    public void SetInfo(string roomCode, int maxPlayerCount, int currentPlayerCount, bool isRoomMaster)
    {
        _roomCode = roomCode;
        _maxPlayerCount = maxPlayerCount;
        _currentPlayerCount = currentPlayerCount;
        _isRoomMaster = isRoomMaster;

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

    private void OnClickStart()
    {
        Managers.Sound.PlayButtonClick();
        LobbyMatchMakingManager.Instance.StartPrivateMatchGame();
    }

    private void OnClickCancel()
    {
        Managers.Sound.PlayButtonClick();
        LobbyMatchMakingManager.Instance.CancelPrivateMatch();
        gameObject.SetActive(false);
    }

    // ─── 내부 ─────────────────────────────────────────────────────────────

    private void Refresh()
    {
        // 방 코드
        GetText((int)Texts.RoomCodeText)?.SetText(_roomCode);

        // 플레이어 수
        GetText((int)Texts.PlayerCountText)?.SetText($"{_currentPlayerCount} / {_maxPlayerCount}");

        // 방장 전용 시작 버튼
        GetObject((int)GameObjects.BtnStart)?.SetActive(_isRoomMaster);
    }
}
