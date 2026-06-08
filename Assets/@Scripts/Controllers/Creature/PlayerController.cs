using CnControls;
using Data;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class PlayerController : BaseController
{
    public Creature parent;
    public Rigidbody _rigidBody { get; set; }

    public bool IsPlayer;
    public SpriteRenderer _glowHead;
    public SpriteRenderer _glowbody;
    public SpriteRenderer _sr;
    public SpriteRenderer _bodysr;
    public AI _ai;

    public GameObject _accessory;

    Vector2 _moveDir = Vector2.zero;
    Vector3 _lastLookDir = Vector3.forward;

    public Action OnPlayerMove;
    public Action OnPlayerDead;

    public ParticleSystem _deadParticle;

    //  충돌 최적화를 위한 캐싱 변수
    private Creature _cachedCreature;
    private Collider _lastCollider;
    private float _lastCollisionTime;

    public Vector2 MoveDir
    {
        get { return _moveDir; }
        set { _moveDir = value.normalized; }
    }

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        Init();
    }

    void OnEnable()
    {
        if (_sr != null)
        {
            var c = _sr.color;
            c.a = 1f;
            _sr.color = c;
        }

        if (_glowHead != null && _glowbody != null)
        {
            var g = _glowHead.color;
            g.a = 0f;
            _glowHead.color = g;

            g.a = 0f;
            _glowbody.color = g;

            _glowHead.gameObject.SetActive(true);
            _glowbody.gameObject.SetActive(true);
        }

        gameObject.tag = "Creature";

        if (_moveDir == Vector2.zero)
            _lastLookDir = transform.forward;

        StartCoroutine(CoSpawnShield());
    }

    void Start()
    {
        // Creature 캐싱
        _cachedCreature = GetComponentInParent<Creature>();

        if (IsPlayer)
        {
            parent.isPlayer = true;
            _sr.sprite = Managers.Resource.Load<Sprite>(Managers.Game.PlayerSpriteNames[0]);

            string spriteName = Managers.Game.PlayerSpriteNames[0].Replace("_Head", "");
            

            _bodysr.sprite = Managers.Resource.Load<Sprite>(spriteName + "_Body.sprite");

        }
        else
        {
            int randomAccessoryID = UnityEngine.Random.Range(0, Managers.Data.AccessoryDic.Count);
            AccessoryData acdata = Managers.Data.AccessoryDic[randomAccessoryID + 1];
            _accessory.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(acdata.SpriteName);

            _sr.sprite = parent._customSprites.sprites[0];
            _bodysr.sprite = parent._customSprites.sprites[1];
        }

   
    }

    int spawnShieldDuration = 2;

    IEnumerator CoSpawnShield()
    {
        parent._activeShield = true;
        parent._ShieldParticle.SetActive(true);

        yield return Define.WAIT_2_SEC;

        parent._activeShield = false;
        parent._ShieldParticle.SetActive(false);
    }

    public override bool Init()
    {
        base.Init();

        ObjectType = Define.EObjectType.Player;

        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;

        transform.localScale = Vector3.one;

        if (IsPlayer)
        {
            int equippedID = Managers.Game.EquippedAccessoryIndex;
            AccessoryData data = Managers.Data.AccessoryDic[equippedID];
            _accessory.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(data.SpriteName);
        }

        return true;
    }

    void Update()
    {
        if (parent.isDead)
            return;

        parent.ControlGlow(_glowHead);
        parent.ControlGlow(_glowbody);

        Move();
        RotateHeadTilt();

        if (Vector3.Distance(transform.position, Vector3.zero) <= 400)
        {
            if (IsPlayer)
                Rotate();
            else
                AIRotate();
        }
        else
        {
            ForceRotateToCenter();
        }

        transform.localScale = new Vector3(parent.referenceScale, parent.referenceScale, parent.referenceScale);
    }

    //  최적화된 OnTriggerEnter
    void OnTriggerEnter(Collider other)
    {
        if (parent.isDead || parent.IsShield)
            return;

        //  중복 충돌 방지 (같은 프레임에 여러 번 호출 방지)
        if (other == _lastCollider && Time.time - _lastCollisionTime < 0.1f)
            return;

        _lastCollider = other;
        _lastCollisionTime = Time.time;

        //  태그 체크 먼저 (가장 빠름)
        if (other.CompareTag("Creature") && other.transform.root != transform.root)
        {
            HandleCreatureCollision(other);
        }
    }

    //  크리처 충돌 처리 분리
    // PlayerController.cs

    void HandleCreatureCollision(Collider other)
    {
        // PlayerController 먼저 체크 (머리)
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            // 상대가 쉴드면 충돌 처리 안 함
            if (controller.parent.IsShield)
                return;

            //  내가 더 크면 내가 이김 (상대가 죽음)
            if (parent.points > controller.parent.points)
            {
                //  내가 플레이어면 킬로그 출력 + 상대 죽이기
                if (IsPlayer)
                {
                    VibrationManager.Inst.VibrateCombo();

                    BodyController body = Managers.Object.Spawn<BodyController>(controller.transform.position, "Body");
                    body.SetInfo(controller.gameObject);
                    body.GetItem();

                    UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
                    ui.SetKillLog(controller.parent._name, parent._name);  // 내가 킬러, 상대가 피해자
                   
                    //  상대 강제로 죽이기
                    controller.Die();
                }
                return;
            }
            // 상대가 더 크거나 같으면 내가 죽음
            else
            {
                // 상대가 플레이어면 킬로그 출력
                if (controller.IsPlayer)
                {
                    VibrationManager.Inst.VibrateCombo();

                    BodyController body = Managers.Object.Spawn<BodyController>(transform.position, "Body");
                    body.SetInfo(gameObject);
                    body.GetItem();

                    UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
                    ui.SetKillLog(parent._name, controller.parent._name);  // 상대가 킬러, 내가 피해자
                }

                Die();
                return;
            }
        }

        // Piece 체크 (꼬리)
        Piece piece = other.GetComponent<Piece>();
        if (piece != null)
        {
            // 상대 머리가 쉴드면 충돌 처리 안 함
            if (piece.Parameters.IsShield)
                return;

            // 꼬리에 부딪히면 무조건 내가 죽음
            if (piece.Parameters._head.IsPlayer)
            {
                VibrationManager.Inst.VibrateCombo();

                BodyController body = Managers.Object.Spawn<BodyController>(transform.position, "Body");
                body.SetInfo(gameObject);
                body.GetItem();

                string enemyname = piece.Parameters._name;

                UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
                ui.SetKillLog(parent._name, enemyname);  // 상대가 킬러, 내가 피해자
            }

            Die();
        }
    }

    void Die()
    {
        if (parent._activeShield)
            return;

        if (IsPlayer)
            _accessory.SetActive(false);

        _glowHead.gameObject.SetActive(false);
        _glowbody.gameObject.SetActive(false);

        parent.Death();
        _deadParticle.Play();
    }

    void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir.normalized;
    }

    private void OnDestroy()
    {
        if (Managers.Game != null)
            Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
    }

    void Move()
    {
        float moveSpeed = parent.speed * parent.speedMultiplier;
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        if (IsPlayer)
            OnPlayerMove?.Invoke();
    }

    void ForceRotateToCenter()
    {
        Vector3 dir = (Vector3.zero - transform.position);
        dir.y = 0f;
        dir.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = targetRot;

        if (IsPlayer)
        {
            _moveDir = dir;
            _lastLookDir = dir;
        }
    }

    [SerializeField] private float _playerRotationSpeed = 15f;
    void Rotate()
    {
        Vector3 axis = new Vector3(CnInputManager.GetAxis("Horizontal"), 0, CnInputManager.GetAxis("Vertical"));

        if (axis != Vector3.zero)
        {
            Vector3 desiredDir = axis.normalized;
            _lastLookDir = desiredDir;

            Quaternion targetLook = Quaternion.LookRotation(_lastLookDir);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetLook,
                _playerRotationSpeed * Time.deltaTime
            );
        }
    }

    void AIRotate()
    {
        if (_ai.direction != Vector3.zero)
        {
            Quaternion targetLook = Quaternion.LookRotation(_ai.direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetLook, parent.rotatingSpeed * Time.deltaTime);
        }
    }

    public void EatAnim()
    {
        _sr.DOKill();

        Sequence seq = DOTween.Sequence();

        // 입 벌리기 (Y축 늘리기)
        seq.Append(_sr.transform.DOScale(new Vector3(1.1f, 1.4f, 1f), 0.15f)
            .SetEase(Ease.OutQuad));

        // 입 다물기 (원래대로)
        seq.Append(_sr.transform.DOScale(new Vector3(1f, 1f, 1f), 0.15f)
            .SetEase(Ease.InOutBack));

        seq.SetUpdate(true);
    }

    void RotateHeadTilt()
    {
        if (_sr == null) return;

        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();

        float tiltInput = Vector3.Dot(_lastLookDir, right);

        float maxTilt = 40f;
        float targetZ = -tiltInput * maxTilt;

        Quaternion baseRot = Quaternion.Euler(90, 0, 0);
        Quaternion tiltRot = Quaternion.Euler(0, 0, targetZ);

        Quaternion finalRot = baseRot * tiltRot;

        _sr.transform.localRotation = Quaternion.Slerp(
            _sr.transform.localRotation,
            finalRot,
            Time.deltaTime * 10f
        );
    }
}