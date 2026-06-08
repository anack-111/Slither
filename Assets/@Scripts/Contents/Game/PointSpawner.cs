using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D; // ✅ SpriteAtlas 사용
using Color = UnityEngine.Color;
using static Define;

public class PointSpawner : MonoBehaviour
{
    public int _pointQuantity = 500;
    public GameObject _point;
    public int _spawnRange = 1000;

    public float playerSpawnRadius = 80f;
    public int minPointsNearPlayer = 120;

    public int initialSpawnBatchSize = 100;
    public int spawnPerMaintainTick = 50;
    public int spawnQueuePerFrame = 50;

    public Camera _mainCam;

    //  SpriteAtlas 추가
    [Header("Sprite Atlas (권장)")]
    public SpriteAtlas foodAtlas; // Inspector에서 할당

    struct SpawnRequest
    {
        public Vector3? pos;
        public float size;
        public bool hasSize;
    }

    Queue<SpawnRequest> _spawnQueue = new Queue<SpawnRequest>();

    public static PointSpawner Inst;
    public Color[] _randomColors;
    public Sprite[] _randomSprites; //  Atlas 미사용 시 fallback
    HashSet<PointController> _recentlyReused = new HashSet<PointController>();

    //  Atlas에서 캐싱한 스프라이트 배열
    private Sprite[] _cachedSprites;

    //  색상 이름 목록 (Atlas의 스프라이트 이름과 일치해야 함)
    private readonly string[] _foodNames = new string[]
    {
        "Food_Orange",
        "Food_Mint",
        "Food_Purple",
        "Food_Red",
        "Food_Yellow",
        "Food_Edit",
        "Food_Pink",
        "Food_blue",

    };

    void Awake()
    {
        Inst = this;

        //  Atlas에서 이름 순서대로 스프라이트 로드
        if (foodAtlas != null)
        {
            _cachedSprites = new Sprite[_foodNames.Length];

            for (int i = 0; i < _foodNames.Length; i++)
            {
                _cachedSprites[i] = foodAtlas.GetSprite(_foodNames[i]);

                if (_cachedSprites[i] == null)
                    Debug.LogWarning($"[PointSpawner] Sprite '{_foodNames[i]}' not found in Atlas!");
            }

            Debug.Log($"[PointSpawner] Loaded {_cachedSprites.Length} sprites from Atlas");
        }
        else if (_randomSprites != null && _randomSprites.Length > 0)
        {
            // Atlas 없으면 기존 배열 사용
            _cachedSprites = _randomSprites;
            Debug.LogWarning("[PointSpawner] No Atlas assigned, using _randomSprites array");
        }
        else
        {
            Debug.LogError("[PointSpawner] No sprites available! Assign foodAtlas or _randomSprites");
        }
    }

    void Start()
    {
        StartCoroutine(Co_SpawnInitialPoints());
        StartCoroutine(Co_MaintainPoints());
        StartCoroutine(Co_ProcessSpawnQueue());
    }

    void LateUpdate()
    {
        _recentlyReused.Clear();
    }

    // ----------------------------------------------
    // INITIAL SPAWN
    // ----------------------------------------------
    IEnumerator Co_SpawnInitialPoints()
    {
        int spawned = 0;

        while (spawned < _pointQuantity)
        {
            int batch = Mathf.Min(initialSpawnBatchSize, _pointQuantity - spawned);

            for (int i = 0; i < batch; i++)
                CreatePoint(null);

            spawned += batch;
            yield return null;
        }
    }

    // ----------------------------------------------
    // MAINTAIN POINT COUNT
    // ----------------------------------------------
    IEnumerator Co_MaintainPoints()
    {
        while (true)
        {
            int count = transform.childCount;
            int spawnedThisTick = 0;

            while (count < _pointQuantity && spawnedThisTick < spawnPerMaintainTick)
            {
                CreatePoint(null);
                count++;
                spawnedThisTick++;
            }

            yield return WAIT_HALF_SEC;
        }
    }

    // ----------------------------------------------
    // PROCESS QUEUE
    // ----------------------------------------------
    IEnumerator Co_ProcessSpawnQueue()
    {
        while (true)
        {
            int processed = 0;

            while (_spawnQueue.Count > 0 && processed < spawnQueuePerFrame)
            {
                SpawnRequest req = _spawnQueue.Dequeue();

                if (req.hasSize)
                    CreatePoint(req.pos, req.size);
                else
                    CreatePoint(req.pos);

                processed++;
            }

            yield return null;
        }
    }

    // ----------------------------------------------
    // POINT CREATION
    // ----------------------------------------------
    Vector3 farvec = new Vector3(9999f, 9999f, 9999f);
    public void CreatePoint(Vector3? spawnPos, float randomSize = 0f, int ColorNumber = -1, bool isDeathSpawn = false)
    {
        float size = (randomSize == 0f)
            ? Random.Range(POINT_MIN_SIZE, POINT_MAX_SIZE)
            : randomSize;

        int currentCount = transform.childCount;

        if (!isDeathSpawn && currentCount >= _pointQuantity)
        {
            if (TryReusePoint(spawnPos, size, ColorNumber, isDeathSpawn))
                return;
            return;
        }

        Vector3 finalPos = (spawnPos.HasValue && spawnPos.Value != Vector3.zero)
            ? spawnPos.Value
            : GetRandomPos();

        PointController obj = Managers.Object.Spawn<PointController>(finalPos, "Point");

        if (obj == null)
        {
            Debug.LogError("[PointSpawner] Failed to spawn PointController");
            return;
        }

        obj.gameObject.layer = LayerMask.NameToLayer("Points");
        Sprite sprite = GetSpriteFromAtlas(ColorNumber);

        obj.Init(size, isDeathSpawn);
        obj.spriteRenderer.sprite = sprite;
        obj.transform.position = finalPos;
        obj.transform.parent = transform;

        //  죽어서 나오는 포인트는 뾱 튀어나오기
        if (isDeathSpawn)
        {
            obj.PopOut();
        }
    }


    //  Atlas에서 스프라이트 가져오기 (랜덤 또는 특정 인덱스)
    Sprite GetSpriteFromAtlas(int index = -1)
    {
        if (_cachedSprites == null || _cachedSprites.Length == 0)
            return null;

        // 특정 인덱스 요청
        if (index >= 0 && index < _cachedSprites.Length)
            return _cachedSprites[index];

        // 랜덤
        return _cachedSprites[Random.Range(0, _cachedSprites.Length)];
    }

    // ----------------------------------------------
    // RANDOM POSITION
    // ----------------------------------------------
    public Vector3 GetRandomPos()
    {
        Vector3 v;

        do
        {
            v = Random.insideUnitSphere * _spawnRange;
            v.y = 0;
        }
        while (v.sqrMagnitude < 10f);

        return v;
    }

    public void RandomPos(Transform t)
    {
        t.position = GetRandomPos();
    }

    // ----------------------------------------------
    // QUEUED SPAWN
    // ----------------------------------------------
    public void EnqueuePointSpawn(Vector3? spawnPos, float randomSize = 0f)
    {
        if (spawnPos.HasValue && spawnPos.Value == Vector3.zero)
            spawnPos = null;

        SpawnRequest req = new SpawnRequest
        {
            pos = spawnPos,
            size = randomSize,
            hasSize = (randomSize != 0f)
        };

        _spawnQueue.Enqueue(req);
    }



    // 재사용 가능한지 체크
    bool IsReusablePoint(PointController p)
    {
        if (p == null) return false;
        if (!p.gameObject.activeInHierarchy) return false;
        if (_recentlyReused.Contains(p)) return false;
        if (p.IsEating) return false;
        if (p.IsMagnetLocked) return false;
        return true;
    }

    // 카메라에 보이는지 체크
    bool IsOffScreen(Vector3 worldPos)
    {
        if (_mainCam == null)
            return true;

        Vector3 vp = _mainCam.WorldToViewportPoint(worldPos);
        if (vp.z < 0f)
            return true;

        return (vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f);
    }

    public void DespawnPoint(PointController point)
    {
        Managers.Object.Despawn(point);
    }


    public PointController CreatePointWithReturn(Vector3? spawnPos, float randomSize = 0f, int ColorNumber = -1)
    {
        float size = (randomSize == 0f)
            ? Random.Range(POINT_MIN_SIZE, POINT_MAX_SIZE)
            : randomSize;

        int currentCount = transform.childCount;

        if (currentCount >= _pointQuantity)
            return null;

        Vector3 finalPos = (spawnPos.HasValue && spawnPos.Value != Vector3.zero)
            ? spawnPos.Value
            : GetRandomPos();

        PointController obj = Managers.Object.Spawn<PointController>(finalPos, "Point");

        if (obj == null)
            return null;

        obj.gameObject.layer = LayerMask.NameToLayer("Points");
        Sprite sprite = GetSpriteFromAtlas(ColorNumber);

        //  죽어서 나오는 건 애니메이션 스킵
        obj.Init(size, true);
        obj.spriteRenderer.sprite = sprite;
        obj.transform.position = finalPos;
        obj.transform.parent = transform;

        return obj;
    }

    bool TryReusePoint(Vector3? spawnPos, float size, int colorNumber = -1, bool isDeathSpawn = false)
    {
        var points = Managers.Object.Points;
        if (points == null || points.Count == 0)
            return false;

        if (_mainCam == null)
            _mainCam = Camera.main;

        Vector3 finalPos = (!spawnPos.HasValue || spawnPos.Value == Vector3.zero)
            ? GetRandomPos()
            : spawnPos.Value;

        PointController candidate = null;
        int maxSamples = 100;
        int visited = 0;

        foreach (var p in points)
        {
            if (visited >= maxSamples)
                break;

            if (!IsReusablePoint(p))
                continue;

            visited++;

            if (IsOffScreen(p.transform.position))
            {
                candidate = p;
                break;
            }
        }

        if (candidate == null)
        {
            visited = 0;

            foreach (var p in points)
            {
                if (visited >= maxSamples)
                    break;

                if (!IsReusablePoint(p))
                    continue;

                visited++;
                candidate = p;
                break;
            }

            if (candidate == null)
                return false;
        }

        Sprite sprite = GetSpriteFromAtlas(colorNumber);

        candidate.Init(size, isDeathSpawn);
        candidate.transform.position = finalPos;
        candidate.transform.parent = transform;
        candidate.spriteRenderer.sprite = sprite;

        //  죽어서 나오는 건 뾱 튀어나오기
        if (isDeathSpawn)
        {
            candidate.PopOut();
        }

        _recentlyReused.Add(candidate);
        return true;
    }
}