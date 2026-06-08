using DG.Tweening;
using DG.Tweening.Core.Easing;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class GameData
{
    public Define.EJoystickType JoystickType = Define.EJoystickType.Flexible;

    public bool BGMOn = true;
    public bool EffectSoundOn = true;

    public int Point = 0;
    public int BodyCount = 0;
    public int MaxKill = 0;
    public float MaxPlayTime = 0f;
    public int TotalKill = 0;  // 총 킬 수 추가

    public int[] PlayerSpriteIdx = new int[5];
    public string[] PlayerSpriteNames = new string[5];

    #region 악세사리
    public Dictionary<int, bool> AccessoryOwned = new Dictionary<int, bool>();
    public int EquippedAccessoryIndex = 1;  // 기본값: 1번 장착
    #endregion
}

public class GameManager
{
    public Vector3 BodyDestination { get; set; }

    public int[] PlayerSpriteIdx
    {
        get => _gameData.PlayerSpriteIdx;
        set
        {
            _gameData.PlayerSpriteIdx = value;
            SaveGame();
        }
    }
    // 총 킬 수 프로퍼티 추가
    public int TotalKill
    {
        get { return _gameData.TotalKill; }
        set
        {
            _gameData.TotalKill = value;
            SaveGame();
        }
    }
    public string[] PlayerSpriteNames
    {
        get => _gameData.PlayerSpriteNames;
        set
        {
            _gameData.PlayerSpriteNames = value;
            SaveGame();
        }
    }

    public int MaxKill
    {
        get { return _gameData.MaxKill; }
        set
        {
            if (value < Kill)
                _gameData.MaxKill = Kill;
            else
                _gameData.MaxKill = value;

            SaveGame();
        }
    }

    public int BodyCount
    {
        get { return _gameData.BodyCount; }
        set
        {
            _gameData.BodyCount = value;
            SaveGame();
        }
    }

    public int Point
    {
        get { return _gameData.Point; }
        set
        {
            _gameData.Point = value;
            SaveGame();
        }
    }

    private int _kill;
    public int Kill
    {
        get => _kill;
        set
        {
            _kill = value;

            if (value > MaxKill)
                MaxKill = value;
            if(value != 0)
            {
                BodyCount++;
                TotalKill++;  // 총 킬 수 증가
            }
              

            OnKill?.Invoke();
        }
    }

    public float MaxPlayTime
    {
        get => _gameData.MaxPlayTime;
        set
        {
            if (value > _gameData.MaxPlayTime)
            {
                _gameData.MaxPlayTime = value;
                SaveGame();
            }
        }
    }

    public Dictionary<int, bool> AccessoryOwned
    {
        get => _gameData.AccessoryOwned;
        set
        {
            _gameData.AccessoryOwned = value;
            SaveGame();
        }
    }

    //  악세사리 구매/소유 변경 메서드 추가
    public void SetAccessoryOwned(int index, bool owned)
    {
        if (_gameData.AccessoryOwned == null)
            _gameData.AccessoryOwned = new Dictionary<int, bool>();

        _gameData.AccessoryOwned[index] = owned;
        SaveGame();
    }

    public int EquippedAccessoryIndex
    {
        get => _gameData.EquippedAccessoryIndex;
        set
        {
            _gameData.EquippedAccessoryIndex = value;
            SaveGame();
        }
    }

    public bool BGMOn
    {
        get { return _gameData.BGMOn; }
        set
        {
            if (_gameData.BGMOn == value)
                return;
            _gameData.BGMOn = value;
            if (_gameData.BGMOn == false)
            {
                Managers.Sound.Stop(ESound.Bgm);
            }
            else
            {
                string name = "Bgm_Lobby";
                if (Managers.Scene.CurrentScene.SceneType == Define.EScene.GameScene)
                    name = "Bgm_Game";

                Managers.Sound.Play(Define.ESound.Bgm, name);
            }
        }
    }

    public bool EffectSoundOn
    {
        get { return _gameData.EffectSoundOn; }
        set { _gameData.EffectSoundOn = value; }
    }

    public GameData _gameData = new GameData();
    public event Action<Vector2> OnMoveDirChanged;
    public event Action OnKill;
    public Action<EItemType, float, float> OnSpeedBuff;
    public Action<EItemType, float, float> OnShieldBuff;

    public bool IsLoaded = false;
    public PlayerController Player { get; set; }

    public String PlayerName;
    Vector2 _moveDir;

    public int PlayerRank;
    public Vector2 MoveDir
    {
        get { return _moveDir; }
        set
        {
            _moveDir = value;
            OnMoveDirChanged?.Invoke(_moveDir);
        }
    }

    public Define.EJoystickType JoystickType
    {
        get { return _gameData.JoystickType; }
        set { _gameData.JoystickType = value; }
    }

    public List<string> EnemyNameMaster = new List<string>
    {
        "포든", "제트", "플레임", "아론", "바로", "마르코", "제로", "몰리",
        "웨스트", "토니", "봄봄", "까누", "도치", "시안", "에린", "우디",
        "제이", "피터", "네로", "렙터", "로저", "세이지", "아놀드", "유리아",
        "언블로", "아이비", "요셉", "월트", "조엘", "카야", "케일", "포카",
        "피아", "헤디","Faker", "Chovy", "Showmaker", "Keria", "Gumayusi", "Zeus", "Oner", "Doran", "Delight", "Peyz",
        "Zeka", "Canyon", "ShowMaker", "Nuguri", "BeryL", "Ruler", "Peanut", "Deft", "Viper", "Scout",
        "Knight", "JackeyLove", "TheShy", "Rookie", "Meiko", "Hope", "Bin", "Elk", "Xiaohu", "Ming",
        "Caps", "Jankos", "Rekkles", "Perkz", "Wunder", "Mikyx", "Upset", "Hylissang", "Humanoid", "Razork",
        "Inspired", "Impact", "CoreJJ", "Doublelift", "Bjergsen", "Jensen", "Blaber", "Vulcan", "FBI", "Huhi",
        "Ssumday", "Closer", "Abbedagge", "Busio", "Hans Sama", "Bwipo", "Alphari", "PowerOfEvil", "Santorin", "Tactical",
        "Licorice", "Svenskeren", "Fudge", "Spica", "Lost", "SwordArt", "Revenge", "Contractz", "Palafox", "Destiny",
        "Stixxay", "WildTurtle", "Smoothie", "Pobelter", "Aphromoo", "Meteos", "Sneaky", "Bang", "Wolf", "Bengi",
        "MaRin", "Duke", "Huni", "Smeb", "Score", "Pray", "GorillA", "Crown", "Ambition", "CuVee",
        "Cuzz", "Teddy", "Effort", "Clid", "Khan", "Tarzan", "Lehends", "Vsta", "Rich", "Sword",
        "Nuclear", "Tusin", "Fly", "SoHwan", "Tempt", "Lindarang", "Beyond", "Joker", "Malrang", "Dove",
        "Bdd", "Life", "Kiin", "Dread", "Kellin", "Quad", "SeongHwan", "Envyy", "Sylvie", "Taeyoon",
        "DuDu", "Perfect", "Moham", "Raptor", "Mickey", "Lava", "Mowgli", "Ucal", "Sangyoon", "SnowFlower",
        "Umti", "Trigger", "Crazy", "Tempt", "Ian", "Secret", "Kuzan", "Leo", "Peter", "Sohwan",
        "Dove", "Hena", "Wiz", "Moojin", "Clear", "Bay", "Execute", "Hoit", "Zzus", "Kellin",
        "Moham", "Envyy", "Raptor", "Lava", "Trigger", "Ian", "Leo", "Wiz", "Bay", "Hoit"
    };

    private List<string> _enemyNamePool = new List<string>();

    public void ResetEnemyNamePoolForNewGame()
    {
        _enemyNamePool.Clear();
        _enemyNamePool.AddRange(EnemyNameMaster);
    }

    public string GetUniqueEnemyName()
    {
        if (_enemyNamePool.Count == 0)
        {
            return "Enemy";
        }

        int idx = UnityEngine.Random.Range(0, _enemyNamePool.Count);
        string name = _enemyNamePool[idx];
        _enemyNamePool.RemoveAt(idx);

        return name;
    }

    public void OnGameOver()
    {
        ResetEnemyNamePoolForNewGame();
        Managers.Scene.LoadScene(Define.EScene.LobbyScene, (Managers.UI.SceneUI as UI_GameScene).gameObject.transform);
    }

    public void Init()
    {
        _path = Application.persistentDataPath + "/SaveData.json";
        _enemyNamePool = new List<string>(EnemyNameMaster);

        //  최초 실행 체크를 Init에서 먼저 처리
        bool isFirstRun = (PlayerPrefs.GetInt("ISFIRST", 1) == 1);

        if (isFirstRun)
        {
            // 최초 실행: 세이브 파일 삭제하고 플래그 변경
            if (File.Exists(_path))
                File.Delete(_path);

            PlayerPrefs.SetInt("ISFIRST", 0); // 즉시 플래그 변경
            PlayerPrefs.Save(); //  즉시 저장
        }

        // LoadGame 실행
        bool loaded = LoadGame();

        if (!loaded)
        {
            // 로드 실패 (최초 실행 포함)
            IsLoaded = true;
            InitializePlayerSprites();
            InitializeAccessoryOwnership();
            SaveGame();
        }
    }
    public void InitializePlayerSprites()
    {
        // 기본 스프라이트 이름 설정
        _gameData.PlayerSpriteNames = new string[5]
        {
        "Duck_001",      // 머리
        "Duck_002",    // 몸통1
        "Duck_002",    // 몸통2
        "Duck_002",    // 몸통3
        "Duck_002"     // 몸통4
        };

 
    }
    public void InitializeAccessoryOwnership()
    {
        if (_gameData.AccessoryOwned == null)
            _gameData.AccessoryOwned = new Dictionary<int, bool>();

        //  Managers.Data가 null인지 체크
        if (Managers.Data == null || Managers.Data.AccessoryDic == null)
        {
            Debug.LogWarning("[GameManager] AccessoryDic is not loaded yet!");
            return;
        }

        // 최신 AccessoryDic 기준으로 모든 아이템 검증
        foreach (var data in Managers.Data.AccessoryDic.Values)
        {
            int index = data.CustomIndex;

            // 새로 추가된 아이템이면 자동으로 "미보유(false)"로 등록
            if (!_gameData.AccessoryOwned.ContainsKey(index))
            {
                // 기본 소유 아이템 (1번)
                if (index == 1)
                    _gameData.AccessoryOwned[index] = true;
                else
                    _gameData.AccessoryOwned[index] = false;
            }
        }

        SaveGame();
    }

    public bool LoadGame()
    {
        //  ISFIRST 체크는 Init()에서 처리하므로 여기서는 제거

        if (File.Exists(_path) == false)
            return false;

        try
        {
            string fileStr = File.ReadAllText(_path);
            GameData data = JsonConvert.DeserializeObject<GameData>(fileStr);
            if (data != null)
                _gameData = data;

            Point = _gameData.Point;

            //  로드 성공 시에도 악세사리 초기화 (새 아이템 추가 대응)
            InitializeAccessoryOwnership();

            IsLoaded = true;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[GameManager] LoadGame failed: " + e.Message);
            return false;
        }
    }

    string _path;

    public void SaveGame()
    {
        string jsonStr = JsonConvert.SerializeObject(_gameData);
        File.WriteAllText(_path, jsonStr);
    }
}