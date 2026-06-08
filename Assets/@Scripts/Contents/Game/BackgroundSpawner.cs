using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PrefabInfo
    {
        public GameObject prefab;
        public int count = 10;
        public float radius = 2f;              // 충돌 반경
        public string collisionGroup = "default"; // 충돌 그룹
        public bool randomRotateZ = false;
    }

    [Header("Prefab Settings")]
    public PrefabInfo lotus;
    public PrefabInfo lotus2;
    public PrefabInfo lotus3;
    public PrefabInfo lotus4;
    public PrefabInfo lotus5;
    public PrefabInfo lotus6;
    public PrefabInfo land1;
    public PrefabInfo land2;


    [Header("Spawn Settings")]
    public float spawnRange = 50f;

    // 저장할 데이터
    struct PlacedData
    {
        public Vector3 pos;
        public float radius;
        public string group;

        public PlacedData(Vector3 p, float r, string g)
        {
            pos = p;
            radius = r;
            group = g;
        }
    }

    List<PlacedData> _placed = new List<PlacedData>();

    // 충돌 규칙 테이블
    Dictionary<(string, string), bool> collisionRules = new Dictionary<(string, string), bool>();

    void Start()
    {
        InitCollisionRules();
        SpawnPrefabGroup(lotus);
        SpawnPrefabGroup(lotus2);
        SpawnPrefabGroup(lotus3);
        SpawnPrefabGroup(lotus4);
        SpawnPrefabGroup(lotus5);
        SpawnPrefabGroup(lotus6);
        //SpawnPrefabGroup(land1);
        //SpawnPrefabGroup(land2);


    }

    // ===============================
    //  충돌 규칙 정의
    // ===============================
    void InitCollisionRules()
    {
        AddRule("lotus", "lotus", true);    // 연꽃끼리는 충돌 O
        AddRule("lotus", "bg", false);      // 연꽃은 돌과 충돌 체크 안 함 (겹쳐도 OK)
        AddRule("bg", "bg", true);          // BG들끼리는 충돌 체크 O
        AddRule("rk", "rk", true);
    }

    void AddRule(string a, string b, bool check)
    {
        collisionRules[(a, b)] = check;
        collisionRules[(b, a)] = check;
    }

    bool ShouldCheckCollision(string a, string b)
    {
        if (collisionRules.TryGetValue((a, b), out bool check))
            return check;

        return true; // 미정이면 기본 충돌 체크
    }

    // ===============================
    //  프리팹 스폰
    // ===============================
    void SpawnPrefabGroup(PrefabInfo info)
    {
        for (int i = 0; i < info.count; i++)
        {
            Vector3 pos;
            if (TryGetNonOverlappingPos(info, out pos))
            {
                // 회전은 프리팹 기본 회전 그대로 사용
                Quaternion rot = info.prefab.transform.rotation;

                GameObject obj = Instantiate(info.prefab, pos, rot);
                obj.transform.SetParent(transform, false);

                _placed.Add(new PlacedData(pos, info.radius, info.collisionGroup));
            }
        }
    }

    // ===============================
    //  충돌 없는 위치 찾기
    // ===============================
    bool TryGetNonOverlappingPos(PrefabInfo info, out Vector3 result)
    {
        const int maxAttempts = 60;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            attempts++;

            Vector2 circle = Random.insideUnitCircle * spawnRange;
            Vector3 candidate = new Vector3(circle.x, 0f, circle.y);

            bool overlap = false;

            foreach (var p in _placed)
            {
                if (!ShouldCheckCollision(info.collisionGroup, p.group))
                    continue; // 충돌 무시 조합

                float minDist = info.radius + p.radius;
                float dist = Vector3.Distance(candidate, p.pos);

                if (dist < minDist)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
            {
                result = candidate;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }
}
