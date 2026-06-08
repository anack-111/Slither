using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Quantum;
using Photon.Realtime; // Player 타입을 쓰기 위해 필요
using Photon.Deterministic; // DeterministicGameMode
using PhotonHashtable = Photon.Client.PhotonHashtable;
/// <summary>
/// Photon Quantum 기반 매치메이킹 매니저
/// 퀵매치 / 프라이빗매치 연결, 방 입장, 세션 시작을 담당
/// </summary>
public class MatchMakingManager : MonoBehaviour, IInRoomCallbacks
{
    static MatchMakingManager _instance;
    public static MatchMakingManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<MatchMakingManager>();
            return _instance;
        }
    }

    [SerializeField] private RuntimeConfig runtimeConfigAsset;
    [SerializeField] private TextAsset[] mapLayoutJsonList;
    [SerializeField] private int selectedMapIndex = -1;

    public RuntimeConfig RuntimeConfigInstance { get; private set; }
    public RealtimeClient RealtimeClient { get; private set; }
    public SessionRunner SessionRunner { get; private set; }

    [Header("매치메이킹 설정")]
    [SerializeField] private int maxPlayerCount = 8;
    [SerializeField] private int minPlayerCount = 2;

    public int MaxPlayerCount => maxPlayerCount;
    public int MinPlayerCount => minPlayerCount;

    private string _tempGUID;
    private bool _isPrivateMatch = false;
    private CancellationTokenSource _autoStartCts;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _tempGUID = Guid.NewGuid().ToString();
        Initialize();
    }

    public void Initialize()
    {
        RealtimeClient = new RealtimeClient()
        {
            AuthValues = new AuthenticationValues { UserId = _tempGUID },
            UserId = _tempGUID
        };
    }

    private void PrepareRuntimeConfig()
    {
        RuntimeConfigInstance = new RuntimeConfig
        {
            Seed = new System.Random().Next(),
            SimulationConfig = runtimeConfigAsset.SimulationConfig,
            Map = runtimeConfigAsset.Map,
            SystemsConfig = runtimeConfigAsset.SystemsConfig,
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // 퀵매치
    // ─────────────────────────────────────────────────────────────────────

    public async UniTask StartQuickMatchAsync(CancellationToken token)
    {
        _isPrivateMatch = false;
        PrepareRuntimeConfig();

        var connectionArguments = new MatchmakingArguments
        {
            PhotonSettings = PhotonServerSettings.Global.AppSettings,
            PluginName = "QuantumPlugin",
            RoomName = null,
            MaxPlayers = MaxPlayerCount,
            CanOnlyJoin = false,
        };

        await StartMatchmakingInternalAsync(connectionArguments, token);
    }

    private async UniTask StartMatchmakingInternalAsync(MatchmakingArguments connectionArguments, CancellationToken token)
    {
        try
        {
            if (RealtimeClient.IsConnected)
                await RealtimeClient.DisconnectAsync();

            connectionArguments.AsyncConfig = new AsyncConfig
            {
                TaskFactory = AsyncConfig.CreateUnityTaskFactory(),
                CancellationToken = token,
            };
            connectionArguments.NetworkClient = RealtimeClient;

            ShowLoading(true);

            RealtimeClient = await MatchmakingExtensions.ConnectToRoomAsync(connectionArguments);
            RealtimeClient.AddCallbackTarget(this);

            ShowLoading(false);
            ShowQuickMatchPopup();

            // 방이 닫힐 때까지 대기
            while (RealtimeClient.CurrentRoom.IsOpen)
            {
                RealtimeClient.Service();
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            HideQuickMatchPopup();
            ShowLoading(true);

            var playerCount = RealtimeClient.CurrentRoom.PlayerCount;
            var sessionRunnerArguments = new SessionRunner.Arguments
            {
                RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                ClientId = _tempGUID,
                RuntimeConfig = RuntimeConfigInstance,
                SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                GameMode = DeterministicGameMode.Multiplayer,
                PlayerCount = playerCount,
                Communicator = new QuantumNetworkCommunicator(RealtimeClient),
                CancellationToken = token,
            };

            SessionRunner = await SessionRunner.StartAsync(sessionRunnerArguments);
            if (SessionRunner == null)
                throw new Exception("Failed to start Quantum session.");
        }
        catch (OperationCanceledException)
        {
            ShowLoading(false);
            HideQuickMatchPopup();
            Clear();
        }
        catch (Exception e) when (e.Message.Contains("DisconnectByClientLogic"))
        {
            ShowLoading(false);
            HideQuickMatchPopup();
            Clear();
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuickMatch] 오류: {e.GetType().Name}: {e.Message}");
            ShowLoading(false);
            HideQuickMatchPopup();
            Clear();
        }
        finally
        {
            RealtimeClient?.RemoveCallbackTarget(this);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 프라이빗 매치
    // ─────────────────────────────────────────────────────────────────────

    public async UniTask StartPrivateMatchAsync(string roomCode, CancellationToken token)
    {
        _isPrivateMatch = true;
        PrepareRuntimeConfig();

        bool isCreatingRoom = string.IsNullOrEmpty(roomCode);
        if (isCreatingRoom)
            roomCode = GenerateRoomCode();

        var connectionArguments = new MatchmakingArguments
        {
            PhotonSettings = PhotonServerSettings.Global.AppSettings,
            PluginName = "QuantumPlugin",
            RoomName = "PRIVATE_" + roomCode,
            MaxPlayers = MaxPlayerCount,
            CanOnlyJoin = !isCreatingRoom,
        };

        await StartPrivateMatchmakingInternalAsync(connectionArguments, token, isCreatingRoom, roomCode);
    }

    private async UniTask StartPrivateMatchmakingInternalAsync(
        MatchmakingArguments connectionArguments,
        CancellationToken token,
        bool isCreatingRoom,
        string roomCode)
    {
        try
        {
            if (RealtimeClient.IsConnected)
                await RealtimeClient.DisconnectAsync();

            connectionArguments.AsyncConfig = new AsyncConfig
            {
                TaskFactory = AsyncConfig.CreateUnityTaskFactory(),
                CancellationToken = token,
            };
            connectionArguments.NetworkClient = RealtimeClient;

            if (isCreatingRoom)
            {
                const int maxRetries = 10;
                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    if (attempt > 0)
                    {
                        await RealtimeClient.DisconnectAsync();
                        Initialize();
                        connectionArguments.NetworkClient = RealtimeClient;
                        connectionArguments.AsyncConfig = new AsyncConfig
                        {
                            TaskFactory = AsyncConfig.CreateUnityTaskFactory(),
                            CancellationToken = token,
                        };
                        roomCode = GenerateRoomCode();
                        connectionArguments.RoomName = "PRIVATE_" + roomCode;
                    }

                    RealtimeClient = await MatchmakingExtensions.ConnectToRoomAsync(connectionArguments);

                    if (RealtimeClient.LocalPlayer.IsMasterClient)
                        break;

                    if (attempt == maxRetries - 1)
                        throw new Exception("방 코드 충돌이 반복됩니다. 잠시 후 다시 시도해주세요.");
                }
            }
            else
            {
                RealtimeClient = await MatchmakingExtensions.ConnectToRoomAsync(connectionArguments);
            }

            RealtimeClient.AddCallbackTarget(this);
            ShowPrivateMatchRoomPopup(roomCode);
        }
        catch (Exception)
        {
            HidePrivateMatchRoomPopup();
            Clear();
            throw;
        }
    }

    public async UniTask StartPrivateMatchGameAsync(CancellationToken token)
    {
        try
        {
            if (RealtimeClient?.CurrentRoom != null)
            {
                RealtimeClient.CurrentRoom.IsOpen = false;
                RealtimeClient.CurrentRoom.IsVisible = false;
            }

            ShowLoading(true);
            var playerCount = RealtimeClient.CurrentRoom.PlayerCount;

            var sessionRunnerArguments = new SessionRunner.Arguments
            {
                RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                ClientId = _tempGUID,
                RuntimeConfig = RuntimeConfigInstance,
                SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                GameMode = DeterministicGameMode.Multiplayer,
                PlayerCount = playerCount,
                Communicator = new QuantumNetworkCommunicator(RealtimeClient),
                CancellationToken = token,
            };

            SessionRunner = await SessionRunner.StartAsync(sessionRunnerArguments);
            if (SessionRunner == null)
                throw new Exception("Failed to start Quantum session.");
        }
        catch (Exception)
        {
            Clear();
            throw;
        }
        finally
        {
            RealtimeClient?.RemoveCallbackTarget(this);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 방장 시작 / 자동 시작
    // ─────────────────────────────────────────────────────────────────────

    public void StartQuickMatchGame()
    {
        if (RealtimeClient?.CurrentRoom != null && RealtimeClient.LocalPlayer.IsMasterClient)
        {
            RealtimeClient.CurrentRoom.IsOpen = false;
            RealtimeClient.CurrentRoom.IsVisible = false;
        }
    }

    private async UniTaskVoid StartGameDelayedAsync(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            if (RealtimeClient?.CurrentRoom != null && RealtimeClient.CurrentRoom.IsOpen)
            {
                RealtimeClient.CurrentRoom.IsOpen = false;
                RealtimeClient.CurrentRoom.IsVisible = false;
            }
        }
        catch (OperationCanceledException) { }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 정리
    // ─────────────────────────────────────────────────────────────────────

    public void Clear()
    {
        if (RealtimeClient?.IsConnected ?? false)
            RealtimeClient.Disconnect();

        SessionRunner?.Shutdown();
        SessionRunner = null;
        RuntimeConfigInstance = null;
        _isPrivateMatch = false;
    }

    public async UniTask ClearAsync(CancellationToken token)
    {
        await RealtimeClient?.DisconnectAsync(new AsyncConfig { CancellationToken = token });
        SessionRunner?.Shutdown();
        SessionRunner = null;
        RuntimeConfigInstance = null;
    }

    private void OnDestroy()
    {
        Clear();
        if (_instance == this) _instance = null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 유틸
    // ─────────────────────────────────────────────────────────────────────

    private string GenerateRoomCode()
    {
        return new System.Random().Next(1000, 10000).ToString();
    }

    // ─────────────────────────────────────────────────────────────────────
    // UI 헬퍼
    // ─────────────────────────────────────────────────────────────────────

    private UI_QuickMatchPopup _quickMatchPopup;
    private UI_PrivateMatchRoomPopup _privateMatchRoomPopup;
    private UI_Loading _loadingUI;

    private void ShowLoading(bool show)
    {
        if (_loadingUI == null)
            _loadingUI = FindFirstObjectByType<UI_Loading>(FindObjectsInactive.Include);
        if (_loadingUI != null)
            _loadingUI.gameObject.SetActive(show);
    }

    private void ShowQuickMatchPopup()
    {
        if (_quickMatchPopup == null)
            _quickMatchPopup = FindFirstObjectByType<UI_QuickMatchPopup>(FindObjectsInactive.Include);
        if (_quickMatchPopup != null)
        {
            _quickMatchPopup.SetInfo(MinPlayerCount, MaxPlayerCount,
                RealtimeClient.CurrentRoom.PlayerCount,
                RealtimeClient.LocalPlayer.IsMasterClient);
            _quickMatchPopup.gameObject.SetActive(true);
        }
    }

    private void HideQuickMatchPopup()
    {
        if (_quickMatchPopup != null)
            _quickMatchPopup.gameObject.SetActive(false);
    }

    private void ShowPrivateMatchRoomPopup(string roomCode)
    {
        if (_privateMatchRoomPopup == null)
            _privateMatchRoomPopup = FindFirstObjectByType<UI_PrivateMatchRoomPopup>(FindObjectsInactive.Include);
        if (_privateMatchRoomPopup != null)
        {
            _privateMatchRoomPopup.SetInfo(roomCode, MaxPlayerCount,
                RealtimeClient.CurrentRoom.PlayerCount,
                RealtimeClient.LocalPlayer.IsMasterClient);
            _privateMatchRoomPopup.gameObject.SetActive(true);
        }
    }

    private void HidePrivateMatchRoomPopup()
    {
        if (_privateMatchRoomPopup != null)
            _privateMatchRoomPopup.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // IInRoomCallbacks
    // ─────────────────────────────────────────────────────────────────────

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (_isPrivateMatch)
        {
            _privateMatchRoomPopup?.RefreshPlayerCount(RealtimeClient.CurrentRoom.PlayerCount);
        }
        else
        {
            _quickMatchPopup?.RefreshPlayerCount(RealtimeClient.CurrentRoom.PlayerCount);

            if (RealtimeClient.LocalPlayer.IsMasterClient
                && RealtimeClient.CurrentRoom.PlayerCount >= MaxPlayerCount)
            {
                _autoStartCts?.Cancel();
                _autoStartCts = new CancellationTokenSource();
                StartGameDelayedAsync(_autoStartCts.Token).Forget();
            }
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        _autoStartCts?.Cancel();
        _autoStartCts = null;

        if (_isPrivateMatch)
            _privateMatchRoomPopup?.RefreshPlayerCount(RealtimeClient.CurrentRoom.PlayerCount);
        else
            _quickMatchPopup?.RefreshPlayerCount(RealtimeClient.CurrentRoom.PlayerCount);
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        bool isMaster = RealtimeClient.LocalPlayer.IsMasterClient;
        if (_isPrivateMatch)
            _privateMatchRoomPopup?.RefreshMaster(isMaster);
        else
            _quickMatchPopup?.RefreshMaster(isMaster);
    }

    public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        // 방 속성이 변경되었을 때 실행할 로직
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        // 플레이어 속성이 변경되었을 때 실행할 로직
    }
}
