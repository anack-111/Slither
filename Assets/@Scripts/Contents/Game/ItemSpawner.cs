using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ItemSpawnData
    {
        public string poolKey;        // 풀링 이름 (ex: "SpeedItem")
        public float weight = 1f;      // 출현 확률
    }

    [Header("Item List")]
    public ItemSpawnData[] items;

    [Header("Spawn Settings")]
    public int maxItemCount = 10;      // 전체 유지 개수
    public float spawnRange = 800f;    // 맵 범위

    [Header("Player Area Settings")]
    public bool enablePlayerAreaCheck = true;
    public float playerSpawnRadius = 60f;
    public int minItemsNearPlayer = 4;

    public static ItemSpawner Inst;

    void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        SpawnInitialItems();
        StartCoroutine(Co_MaintainItems());
    }

    // 최초 스폰
    void SpawnInitialItems()
    {
        for (int i = 0; i < maxItemCount; i++)
            CreateItem();
    }

    // 전체 개수 유지
    IEnumerator Co_MaintainItems()
    {
        while (true)
        {
            int count = transform.childCount;

            while (count < maxItemCount)
            {
                CreateItem();
                count++;
            }

            if (enablePlayerAreaCheck)
                SpawnNearPlayerIfNeeded();

            yield return Define.WAIT_HALF_SEC;
        }
    }

    // 플레이어 주변 보충
    void SpawnNearPlayerIfNeeded()
    {
        if (Managers.Object.Player == null)
            return;

        Vector3 playerPos = Managers.Object.Player.transform.position;
        int count = 0;

        foreach (Transform child in transform)
        {
            if (Vector3.Distance(child.position, playerPos) <= playerSpawnRadius)
                count++;
        }

        while (count < minItemsNearPlayer)
        {
            SpawnAroundPlayer(playerPos);
            count++;
        }
    }

    // item 생성 (풀링)
    public void CreateItem(Vector3 pos = default)
    {
        string key = PickRandomItemKey();
        if (key == null) return;

        // 풀링 Spawn
        ItemController obj = Managers.Object.Spawn<ItemController>(Vector3.zero, key);

        if (pos != default)
            obj.transform.position = pos;
        else
            obj.transform.position = RandomPos();

        obj.transform.SetParent(transform, true);

    }

    // 풀링 Despawn
    public void DespawnItem(ItemController item)
    {
        Managers.Object.Despawn(item);
    }

    // 아이템 랜덤 선택
    string PickRandomItemKey()
    {
        float total = 0f;
        foreach (var item in items)
            total += item.weight;

        float r = Random.Range(0, total);
        float sum = 0;

        foreach (var item in items)
        {
            sum += item.weight;
            if (r <= sum)
                return item.poolKey;
        }

        return items[0].poolKey;
    }

    // 맵 랜덤 위치
    Vector3 RandomPos()
    {
        Vector3 v = Random.insideUnitSphere * spawnRange;
        v.y = 0;
        return v;
    }

    // 플레이어 주변 랜덤 생성
    void SpawnAroundPlayer(Vector3 center)
    {
        string key = PickRandomItemKey();
        ItemController obj = Managers.Object.Spawn<ItemController>(Vector3.zero, key);

        Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(30f, playerSpawnRadius);
        Vector3 pos = new Vector3(circle.x, 0, circle.y) + center;

        obj.transform.position = pos;
        obj.transform.SetParent(transform);
    }
}
