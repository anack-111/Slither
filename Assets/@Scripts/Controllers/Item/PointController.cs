using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PointController : ItemController
{
    [SerializeField] float PointValue;
    public bool IsEating;

    bool destruction;
    public SpriteRenderer spriteRenderer;

    public bool IsMagnetLocked = false;

    Vector3 circleOffset;
    float circleSpeed;
    float circleRadius;
    float angle;

    Vector3 _targetScale;

    public PlayerController magnetOwner;

    bool _isSpawning;

    private static Camera s_mainCamera;

    public GameObject _trailObject;

    private Coroutine _spawnAppearCoroutine;

    //  프레임 오프셋 (분산 체크용)
    private int _frameOffset;

    private void Awake()
    {
        circleRadius = Random.Range(0.5f, 1f);
        circleSpeed = Random.Range(1f, 2f);
        angle = Random.Range(0f, 360f);

        //  프레임 오프셋 (모든 포인트가 같은 프레임에 체크 안 하게)
        _frameOffset = Random.Range(0, 10);

        if (s_mainCamera == null)
            s_mainCamera = Camera.main;

        //  Points 레이어 설정
        gameObject.layer = LayerMask.NameToLayer("Points");
    }

    public void Init(float value, bool skipSpawnAnimation = false)
    {
        PointValue = value;
        Vector3 finalScale = new Vector3(1 + value, 1 + value, 1 + value);

        _targetScale = finalScale / 1.3f;
        IsMagnetLocked = false;
        IsEating = false;

        if (_spawnAppearCoroutine != null)
            StopCoroutine(_spawnAppearCoroutine);

        if (skipSpawnAnimation)
        {
            transform.localScale = _targetScale;
            _isSpawning = false;
        }
        else
        {
            _spawnAppearCoroutine = StartCoroutine(CO_SpawnAppear());
        }
    }
    Vector3 farvec =  new Vector3(9999f, 9999f, 9999f);
    void OnEnable()
    {
        _trailObject.SetActive(false);
        transform.position = farvec;
    }

    void OnDisable()
    {
        if (_spawnAppearCoroutine != null)
        {
            StopCoroutine(_spawnAppearCoroutine);
            _spawnAppearCoroutine = null;
        }

        transform.DOKill();
    }

    //  최적화된 Update
    void Update()
    {
        //  10프레임마다 분산 체크 (렌더러 on/off)
        if ((Time.frameCount + _frameOffset) % 10 == 0)
        {
            bool isVisible = IsVisibleOnScreen();
            if (spriteRenderer.enabled != isVisible)
                spriteRenderer.enabled = isVisible;
        }

        // 화면 밖이면 애니메이션 스킵
        if (!spriteRenderer.enabled) return;
        if (IsEating || IsMagnetLocked) return;

        // 4프레임마다 1번 실행 (애니메이션)
        if (Time.frameCount % 4 != 0) return;

        angle += Time.deltaTime * circleSpeed * 2f;

        float x = Mathf.Cos(angle) * circleRadius;
        float z = Mathf.Sin(angle) * circleRadius;

        circleOffset = new Vector3(x, 0, z);

        transform.position += circleOffset * Time.deltaTime * 2f;
    }

    //  화면에 보이는지 체크
    bool IsVisibleOnScreen()
    {
        if (s_mainCamera == null)
            s_mainCamera = Camera.main;

        if (s_mainCamera == null)
            return false;

        Vector3 viewportPoint = s_mainCamera.WorldToViewportPoint(transform.position);

        // z가 음수면 카메라 뒤에 있음
        if (viewportPoint.z < 0)
            return false;

        // 화면 범위 체크 (약간 여유 있게)
        return viewportPoint.x >= -0.1f && viewportPoint.x <= 1.1f &&
               viewportPoint.y >= -0.1f && viewportPoint.y <= 1.1f;
    }

    public void SetColor(Color col)
    {
        spriteRenderer.color = col;
    }

    protected override void OnTriggerEnter(Collider obj)
    {
        var creature = obj.GetComponentInParent<Creature>();
        if (creature == null)
            return;

        if (IsMagnetLocked)
        {
            var player = creature.GetComponentInChildren<PlayerController>();
            if (player != magnetOwner)
                return;
        }

        if (obj.CompareTag("Creature"))
        {
            creature.AddPoints(Mathf.RoundToInt(PointValue * 2));
            creature._head.EatAnim();

            StartCoroutine(CO_MagnetToPlayer(obj.transform));
        }
    }

    public void LockToMagnet(PlayerController owner)
    {
        IsMagnetLocked = true;
        magnetOwner = owner;
    }

    public IEnumerator CO_MagnetToPlayer(Transform targetTransform, bool ismagnet = false)
    {
        IsEating = true;

        float lerper = 0;
        float lerperTime;

        if (ismagnet)
            lerperTime = Random.Range(0.35f, 0.6f);
        else
            lerperTime = Random.Range(0.2f, 0.4f);

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        while (lerper <= 1f)
        {
            lerper += Time.deltaTime / lerperTime;

            if (targetTransform == null)
                break;

            transform.position = Vector3.Lerp(startPos, targetTransform.position - Vector3.up, lerper);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, lerper);

            yield return null;
        }

        IsEating = false;

        if (PointSpawner.Inst != null)
            PointSpawner.Inst.DespawnPoint(this);
        else
            Managers.Object.Despawn(this);
    }

    IEnumerator CO_SpawnAppear()
    {
        _isSpawning = true;

        float lerper = 0f;
        float lerperTime = 1f;

        Vector3 startScale = Vector3.zero;
        Vector3 endScale = _targetScale;

        transform.localScale = startScale;

        while (lerper < 1f)
        {
            lerper += Time.deltaTime / lerperTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, lerper);
            yield return null;
        }

        transform.localScale = endScale;
        _isSpawning = false;

        _spawnAppearCoroutine = null;
    }

    public void ScaleBounceThenMagnet(Transform target)
    {
        if (IsEating)
            return;

        Vector3 originalScale = transform.localScale;

        float bounceScale = Random.Range(1.35f, 1.5f);
        float bounceTime = Random.Range(0.2f, 0.3f);

        transform.DOKill();

        transform.DOScale(originalScale * bounceScale, bounceTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(originalScale, bounceTime * 0.7f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        if (!isActiveAndEnabled)
                            return;

                        if (target == null || !target.gameObject.activeInHierarchy)
                            return;

                        if (!gameObject.activeInHierarchy)
                            return;

                        StartCoroutine(CO_MagnetToPlayer(target, true));
                    });
            });
    }




    public void PopOut()
    {
        StartCoroutine(CO_PopOut());
    }

    IEnumerator CO_PopOut()
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = _targetScale;
        Vector3 overScale = _targetScale * 1.3f; // 살짝 크게 튕기기

        float duration = 0.2f;
        float elapsed = 0f;

        transform.localScale = startScale;

        //  뾱하고 커지기 (0 → 크게 → 정상)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t < 0.6f)
            {
                // 처음 60%: 빠르게 크게 커짐
                float scaleT = t / 0.6f;
                transform.localScale = Vector3.Lerp(startScale, overScale, scaleT);
            }
            else
            {
                // 나머지 40%: 정상 크기로
                float scaleT = (t - 0.6f) / 0.4f;
                transform.localScale = Vector3.Lerp(overScale, endScale, scaleT);
            }

            yield return null;
        }

        transform.localScale = endScale;
    }
}