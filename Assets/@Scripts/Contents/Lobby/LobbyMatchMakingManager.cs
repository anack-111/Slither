using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Quantum;

/// <summary>
/// 로비 씬에서 매치메이킹 흐름을 총괄하는 매니저
/// - MatchMakingManager(Photon 연결)와 UI 사이의 중간 레이어
/// - UniTask 기반 비동기 처리
/// </summary>
public class LobbyMatchMakingManager : MonoBehaviour
{
    static LobbyMatchMakingManager _instance;
    public static LobbyMatchMakingManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<LobbyMatchMakingManager>();
            return _instance;
        }
    }

    private CancellationTokenSource _cts;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        if (_instance == this) _instance = null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 퀵매치
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>퀵매치 시작 (버튼에서 호출)</summary>
    public void StartQuickMatch()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        RunQuickMatch(_cts.Token).Forget();
    }

    /// <summary>방장이 시작 버튼 눌렀을 때 → 방 닫아서 모든 클라이언트에 게임 시작 신호</summary>
    public void StartQuickMatchGame()
    {
        MatchMakingManager.Instance.StartQuickMatchGame();
    }

    /// <summary>퀵매치 취소</summary>
    public void CancelQuickMatch()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        MatchMakingManager.Instance.Clear();
    }

    private async UniTaskVoid RunQuickMatch(CancellationToken token)
    {
        try
        {
            await MatchMakingManager.Instance.StartQuickMatchAsync(token);

            // 세션 시작 성공 → 플레이어 추가 후 게임씬 전환
            QuantumRunner.DefaultGame.AddPlayer(new RuntimePlayer());
            LoadGameScene();
        }
        catch (OperationCanceledException)
        {
            // 취소 (정상)
        }
        catch (Exception e)
        {
            Debug.LogError($"[LobbyMatchMakingManager] QuickMatch 오류: {e.Message}");
            Managers.UI.ShowToast("매칭 중 오류가 발생했습니다.");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 프라이빗 매치
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>프라이빗 매치 시작 (roomCode == null이면 방 생성, 아니면 참가)</summary>
    public void StartPrivateMatch(string roomCode)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        RunPrivateMatch(roomCode, _cts.Token).Forget();
    }

    /// <summary>방장이 게임 시작 버튼 눌렀을 때</summary>
    public void StartPrivateMatchGame()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        RunPrivateMatchGame(_cts.Token).Forget();
    }

    /// <summary>프라이빗 매치 취소 (방 나가기)</summary>
    public void CancelPrivateMatch()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        MatchMakingManager.Instance.Clear();
    }

    private async UniTaskVoid RunPrivateMatch(string roomCode, CancellationToken token)
    {
        try
        {
            // 로딩 표시는 MatchMakingManager 내부에서 처리
            await MatchMakingManager.Instance.StartPrivateMatchAsync(roomCode, token);
            // 성공 시 PrivateMatchRoomPopup이 표시된 상태로 대기
        }
        catch (OperationCanceledException)
        {
            // 취소 (정상)
        }
        catch (Exception e)
        {
            string errorMsg = "방 입장에 실패했습니다.";

            if (e.Message.Contains("no match found") ||
                e.Message.Contains("room not found") ||
                e.Message.Contains("JoinRandomFailed"))
            {
                errorMsg = "방을 찾을 수 없습니다!";
            }
            else if (e.Message.Contains("full"))
            {
                errorMsg = "방이 가득 찼습니다!";
            }
            else if (e.Message.Contains("방 코드 충돌"))
            {
                errorMsg = e.Message;
            }

            Managers.UI.ShowToast(errorMsg);
            Debug.LogError($"[LobbyMatchMakingManager] PrivateMatch 오류: {e.Message}");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    private async UniTaskVoid RunPrivateMatchGame(CancellationToken token)
    {
        try
        {
            await MatchMakingManager.Instance.StartPrivateMatchGameAsync(token);

            // 세션 시작 성공 → 플레이어 추가 후 게임씬 전환
            QuantumRunner.DefaultGame.AddPlayer(new RuntimePlayer());

            // PrivateMatchRoomPopup 닫기
            var roomPopup = FindFirstObjectByType<UI_PrivateMatchRoomPopup>(FindObjectsInactive.Include);
            if (roomPopup != null)
                roomPopup.gameObject.SetActive(false);

            LoadGameScene();
        }
        catch (OperationCanceledException)
        {
            // 취소 (정상)
        }
        catch (Exception e)
        {
            Debug.LogError($"[LobbyMatchMakingManager] PrivateMatchGame 오류: {e.Message}");
            Managers.UI.ShowToast("게임 시작 중 오류가 발생했습니다.");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 씬 전환
    // ─────────────────────────────────────────────────────────────────────

    private void LoadGameScene()
    {
        Managers.Scene.LoadScene(Define.EScene.GameScene, transform);
    }
}
