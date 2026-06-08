// Piece.cs
using UnityEngine;
using System.Collections;

public class Piece : BaseController
{
    private Sprite _myOpenSprite;
    private Sprite _myClosedSprite;
    bool _isDuck002Piece;

    public int PieceIndex;
    public Creature Parameters;
    Transform reference;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer shadowSprite;
    [SerializeField] float pieceDistanceOffset = 0.5f;
    [SerializeField] float firstPieceExtraOffset = 0.2f;
    [SerializeField] float gapExtraFactor = 0.3f;

    float pieceYDistanceOffset = 0.01f;

    public SpriteRenderer _glow;
    public GameObject _shadowObject;
    public ParticleSystem _ground;

    float _cachedScale = -999f;
    float _cachedDistance = -999f;

    Vector3 _cachedScaleVec;
    Vector3 _referencePosition;
    Quaternion _cachedRot;

    public bool IsPulsing = false;

    float followSpeed = 10f;
    float rotationSpeed = 35f;  //  회전 속도

    public void Init(int index, Creature parameters)
    {
        PieceIndex = index;
        Parameters = parameters;

        transform.parent = Parameters.transform;
        reference = Parameters.Ducks[index - 1];

        float distance = GetDistanceCached();

        transform.position =
            reference.position
            - reference.forward * distance
            - Vector3.up * pieceYDistanceOffset;

        SetSpriteBasedOnIndex();
       // ResetGroundEffect();

        Parameters.OnSprintChanged += SetAnim;
        SetAnim(parameters._wasSprinting);
    }

    private void OnEnable()
    {
        gameObject.tag = "Creature";

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            shadowSprite.enabled = true;
            var col = spriteRenderer.color;
            col.a = 1f;
            spriteRenderer.color = col;
        }

        if (_glow != null)
        {
            var gcol = _glow.color;
            gcol.a = 0f;
            _glow.color = gcol;
        }

        _shadowObject.SetActive(true);
    }

    private void OnDisable()
    {
        //if (_ground != null)
        //    _ground.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        Parameters.OnSprintChanged -= SetAnim;
    }

    int frameSkip = 1;
    int counter = 0;

    void Update()
    {
        //  AI 크리처의 피스는 부모가 안 보이면 업데이트 스킵
        if (Parameters != null && !Parameters.isPlayer)
        {
            if (Parameters._head != null && Parameters._head._sr != null)
            {
                if (!Parameters._head._sr.enabled)
                    return;
            }
        }

        if (++counter % frameSkip != 0)
            return;

        if (Parameters == null)
            return;

        MoveOptimized();
        RotateSmooth();  //  부드러운 회전

        Parameters.ControlGlow(_glow);

        if (!IsPulsing)
        {
            float scale = Parameters.referenceScale;
            if (Mathf.Abs(scale - _cachedScaleVec.x) > 0.0001f)
            {
                _cachedScaleVec = new Vector3(scale, scale, scale);
                transform.localScale = _cachedScaleVec;
            }
        }
    }

    void EnsureDuck002Sprites()
    {
        //  이미 로드했으면 리턴
        if (_myOpenSprite != null && _myClosedSprite != null)
            return;

        int spriteIndex = (PieceIndex - 1) % 4 + 1;
        string basename = Managers.Game.PlayerSpriteNames[spriteIndex];

        //  각 Piece가 자신의 스프라이트 로드
        _myOpenSprite = Managers.Resource.Load<Sprite>($"{basename}.sprite");
        _myClosedSprite = Managers.Resource.Load<Sprite>($"{basename}_EyesClosed.sprite");
    }

    float GetDistanceCached()
    {
        float scale = Parameters.referenceScale;
        if (Mathf.Abs(scale - _cachedScale) < 0.0001f)
            return _cachedDistance;

        _cachedScale = scale;

        float extra = 1f + gapExtraFactor * (scale - 1f);
        float dist = pieceDistanceOffset * scale * extra;

        if (PieceIndex == 1)
            dist += firstPieceExtraOffset * extra * 2f;

        _cachedDistance = dist;
        return dist;
    }

    void MoveOptimized()
    {
        float distance = GetDistanceCached();

        Transform refT = reference;
        Vector3 refPos = refT.position;
        Vector3 fwd = refT.forward;

        _referencePosition = refPos - fwd * distance;
        _referencePosition.y = transform.position.y;

        float speedMul = Parameters.speedMultiplier;

        //  스프린트 시 followSpeed가 증가 (10 → 20)
        float sprintFollow = followSpeed * Mathf.Lerp(1f, 2f, (speedMul - 1f) * 0.5f);

        float t = sprintFollow * Time.deltaTime;
        if (t > 1f) t = 1f;

        transform.position = Vector3.Lerp(transform.position, _referencePosition, t);
    }

    //  부드러운 회전
    void RotateSmooth()
    {
        Vector3 direction = _referencePosition - transform.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        //  Slerp로 부드럽게 회전
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    void ResetGroundEffect()
    {
        if (_ground == null)
            return;

        _ground.gameObject.SetActive(true);
        _ground.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _ground.Clear(true);
        _ground.Play(true);
    }

    void SetSpriteBasedOnIndex()
    {
        if (Parameters.isPlayer)
        {
            EnsureDuck002Sprites();

            int spriteIndex = (PieceIndex - 1) % 4 + 1;
            spriteRenderer.sprite = Managers.Resource.Load<Sprite>(Managers.Game.PlayerSpriteNames[spriteIndex]);

            //  내 스프라이트와 비교
            _isDuck002Piece =
                (spriteRenderer.sprite == _myOpenSprite) ||
                (spriteRenderer.sprite == _myClosedSprite);

            if (_isDuck002Piece)
                spriteRenderer.sprite = _myClosedSprite;
        }
        else
        {
            spriteRenderer.sprite = Parameters._customSprites.sprites[2];
        }
    }

    public void UpdateReference()
    {
        if (PieceIndex <= 0) return;
        if (Parameters == null) return;
        if (Parameters.Ducks == null) return;

        // 인덱스 범위 체크
        if (PieceIndex - 1 >= 0 && PieceIndex - 1 < Parameters.Ducks.Count)
        {
            reference = Parameters.Ducks[PieceIndex - 1];
        }
        else
        {
            Debug.LogWarning($"[Piece] UpdateReference - Invalid index: {PieceIndex}, Ducks.Count: {Parameters.Ducks.Count}");
        }
    }

    public void TurnOffShadow()
    {
        _shadowObject.SetActive(false);
       // _ground.gameObject.SetActive(false);
    }

    public void SetAnim(bool isSprint)
    {
        if (Parameters.isPlayer)
        {
            if (!_isDuck002Piece)
                return;

            EnsureDuck002Sprites();

            //  내 스프라이트 사용
            spriteRenderer.sprite = isSprint ? _myOpenSprite : _myClosedSprite;
            return;
        }

        if (isSprint)
            spriteRenderer.sprite = Parameters._customSprites.sprites[3];
        else
            spriteRenderer.sprite = Parameters._customSprites.sprites[2];
    }
}