using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AISpawner : MonoBehaviour
{
    public static AISpawner Inst;

    public int small = 1;
    public int medium = 5;
    public int big = 7;
    public float spawnCircleLenght = 1000;
    public GameObject Prefab;

    [Header("Spawn queue settings")]
    public int spawnQueuePerFrame = 2; // how many spawns to process per frame
    public int prewarmPoolCount = 0; // if >0, prewarm pool at Start
    public static bool IsInitialSpawn { get; private set; } = true;

    Queue<int> _spawnQueue = new Queue<int>();

    void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        // Enqueue initial spawns instead of immediate heavy instantiation
        for (int i = 0; i < small; i++)
            EnqueueCreatureSpawn(Random.Range(500, 700));

        for (int i = 0; i < medium; i++)
            EnqueueCreatureSpawn(Random.Range(800, 1500));

        for (int i = 0; i < big; i++)
            EnqueueCreatureSpawn(Random.Range(2000, 2500));

        // Start processing queue
        StartCoroutine(Co_ProcessSpawnQueue());

        // Optional: prewarm pool to avoid Instantiate spikes during gameplay
        if (prewarmPoolCount > 0 && Prefab != null)
        {
            StartCoroutine(Co_PrewarmPool());
        }


        StartCoroutine(Co_FinishInitialSpawn());
    }
    IEnumerator Co_FinishInitialSpawn()
    {
        yield return new WaitForSeconds(2f);
        IsInitialSpawn = false;
        Debug.Log("[AISpawner] Initial spawn finished - Sprint points enabled");
    }

    IEnumerator Co_PrewarmPool()
    {
        // Spread prewarm across frames to avoid start-up spikes
        for (int i = 0; i < prewarmPoolCount; i++)
        {
            GameObject go = Managers.Pool.Pop(Prefab);
            // Immediately return to pool
            Managers.Pool.Push(go);
            if (i % 10 == 0) // every 10 items yield a frame
                yield return null;
        }
        yield break;
    }

    // Public enqueue method
    public void EnqueueCreatureSpawn(int points)
    {
        _spawnQueue.Enqueue(points);
    }

    // Processes queued spawn requests over frames
    IEnumerator Co_ProcessSpawnQueue()
    {
        while (true)
        {
            int processed = 0;
            while (_spawnQueue.Count > 0 && processed < spawnQueuePerFrame)
            {
                int points = _spawnQueue.Dequeue();
                SpawnCreatureImmediate(points);
                processed++;
            }
            // Spread across frames
            yield return null;
        }
    }

    // Actual spawn implementation (kept private)
    void SpawnCreatureImmediate(int points)
    {
        if (Prefab == null)
        {
            Debug.LogError("[AISpawner] Prefab is missing");
            return;
        }

        // 1) Pool Pop
        GameObject creature = Managers.Pool.Pop(Prefab);
        if (creature == null)
        {
            Debug.LogError("[AISpawner] Pool Pop failed");
            return;
        }

        // Ensure position/rotation are set BEFORE OnEnable side-effects by briefly deactivating
        bool wasActive = creature.activeSelf;
        if (wasActive)
            creature.SetActive(false);

        // 2) base position
        creature.transform.position = Prefab.transform.position;
        creature.transform.rotation = Prefab.transform.rotation;

        // 3) random offset
        Vector2 randomSpawnCircleVector2 = Random.insideUnitCircle * spawnCircleLenght;
        Vector3 randomSpawnCircle = new Vector3(
            randomSpawnCircleVector2.x,
            creature.transform.position.y,
            randomSpawnCircleVector2.y);

        creature.transform.position += randomSpawnCircle;

        // Reactivate so OnEnable runs with correct position
        creature.SetActive(true);

        // 4) configure creature
        Creature param = creature.GetComponent<Creature>();
        if (param != null)
        {
            param.points = points;

            if (param._head != null)
                param._head.IsPlayer = false;

            param.isPlayer = false;
            param.isDead = false;  // OnEnable will run when activated

            param.Init();

            Managers.Object.RegisterCreature(param);
        }
    }

    // Legacy immediate spawn (kept for compatibility if needed)
    public void SpawnCreature(int points)
    {
        // Backwards-compatible wrapper that enqueues
        EnqueueCreatureSpawn(points);
    }
}
