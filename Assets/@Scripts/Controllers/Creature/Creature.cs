// Creature.cs
using CnControls;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static Define;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour
{
    public Action<int> OnPointsChanged;
    public GameObject PartPrefab;

    [Header("Creature Value Setting")]

    public float speed = 5;
    public float rotatingSpeed = 0.7f;
    public int points;
    public float speedMultiplier = 2;
    public int pieceForPoints = 50;
    int startingPoints = 500;

    float originalSpeedMultiplier;

    public PlayerController _head;

    public EColor _color;
    public static Creature parent;
    public bool isPlayer;
    public bool isDead;

    int pointsForScale = 100;
    float scaleOffset = 0.05f;
    public float referenceScale;
    public AI _ai;
    public string _name;


    #region 파티클 및 그림자
    public CustomSprite _customSprites;
    public UI_PlayerName _uiPlayerName;
    public ParticleSystem _waveParitcle;
    public GameObject _bubbleParticle;
    public GameObject _shieldParticle;
    public GameObject _buffParticle;
    public GameObject _sprintParticle;

    [Header("Renderer Particle")]
    public GameObject _shadowObject;
    public GameObject _bodyshadowObject;

    [Header("Item Particle")]
    public GameObject _SpeedParticle;
    public GameObject _ShieldParticle;
    public GameObject _DeathAnim;
    public SpriteRenderer _bodySr;
    #endregion

    public List<Transform> Ducks = new List<Transform>();

    //  피스 캐싱
    private List<Piece> _cachedPieces = new List<Piece>();

    bool _ActiveSpeedBuff = false;
    float originalSprintMultiplier = 2f;
    float _speedBuffValue = 1f;


    Coroutine _sprintCo;
    public bool _wasSprinting = false;

    bool _isWaveFast = false;


    public event Action<bool> OnSprintChanged;
    UI_GameScene _uigamescene;



    void Awake()
    {
        RandomSprite();
        originalSpeedMultiplier = speedMultiplier;
    }

    public void Start()
    {
        if (_head.IsPlayer)
        {
            _name = Managers.Game.PlayerName;
            _uiPlayerName.SetInfo(_head);
            _color = EColor.Yellow;

            _uigamescene = Managers.UI.SceneUI as UI_GameScene;
        }
    }



    void OnEnable()
    {
        isDead = false;
        _wasSprinting = false;
        speedMultiplier = originalSpeedMultiplier;
        referenceScale = 1f;
        _isWaveFast = false;

        if (Ducks == null)
            Ducks = new List<Transform>();
        Ducks.Clear();

        //  캐시도 클리어
        _cachedPieces.Clear();

        if (_head != null)
            Ducks.Add(_head.transform);

        #region 파티클 초기화

        ToggleParticle(true);

        #endregion

        if (_sprintCo != null)
            StopCoroutine(_sprintCo);
        _sprintCo = StartCoroutine(CO_Sprint());

        _SpeedParticle.SetActive(false);
        _ShieldParticle.SetActive(false);
    }

    public void Init()
    {
        if (_uiPlayerName != null)
        {
            if (_head.IsPlayer)
            {
                _name = Managers.Game.PlayerName;
                _uiPlayerName.SetInfo(_head);

            }
            else
            {
                string name = Managers.Game.GetUniqueEnemyName();
                _name = name;
                _uiPlayerName.SetInfo(_head);

            }
        }
    }

    void OnDisable()
    {
        if (_sprintCo != null)
        {
            StopCoroutine(_sprintCo);
            _sprintCo = null;
        }
    }

    void RandomSprite()
    {
        CustomSprite[] spriteTemplates = FindObjectsOfType<CustomSprite>();

        if (spriteTemplates.Length == 0)
            return;

        int random = Random.Range(0, spriteTemplates.Length);
        _color = spriteTemplates[random].color;
        _customSprites = spriteTemplates[random];
    }



    void Update()
    {
        if (isDead)
            return;

        CreatureSize();
        CreatureScale();
        CreatureSprint();

        // AI 크리처 렌더링 최적화 (10프레임마다)
        if (!isPlayer && Time.frameCount % 5 == 0)
        {
            OptimizeAIRendering();
        }

        if (!isPlayer)
            return;

        if (isComboActive)
        {
            comboTimer += Time.deltaTime;

            if (comboTimer >= comboDuration)
            {
                currentCombo = 0;
                isComboActive = false;
            }
        }
    }

    void OptimizeAIRendering()
    {
        if (_head == null || Camera.main == null || Ducks.Count == 0) return;

        float headDist = Vector3.Distance(_head.transform.position, Camera.main.transform.position);

        float tailDist = headDist;
        if (Ducks.Count > 1)
        {
            Transform tail = Ducks[Ducks.Count - 1];
            tailDist = Vector3.Distance(tail.position, Camera.main.transform.position);
        }

        float closestDist = Mathf.Min(headDist, tailDist);

        if (closestDist > 300f)
        {
            // 완전히 멀리: 최적화
            if (_head._sr != null)
                _head._sr.enabled = false;

            //  캐싱된 리스트 사용 (GetComponent 없음!)
            for (int i = 0; i < _cachedPieces.Count; i++)
            {
                Piece piece = _cachedPieces[i];
                if (piece != null)
                {
                    piece.spriteRenderer.enabled = false;
                    piece.shadowSprite.enabled = false;
                }
            }

            if (_waveParitcle != null && _waveParitcle.gameObject.activeSelf)
                _waveParitcle.gameObject.SetActive(false);
        }
        else
        {
            // 가까이 있음: 전부 렌더링
            if (_head._sr != null)
                _head._sr.enabled = true;

            //  캐싱된 리스트 사용
            for (int i = 0; i < _cachedPieces.Count; i++)
            {
                Piece piece = _cachedPieces[i];
                if (piece != null)
                {
                    piece.spriteRenderer.enabled = true;
                    piece.shadowSprite.enabled = true;
                }
            }

            // 파티클은 더 가까울 때만
            bool showParticles = closestDist < 150f;
            if (_waveParitcle != null && _waveParitcle.gameObject.activeSelf != showParticles)
                _waveParitcle.gameObject.SetActive(showParticles);
        }
    }

    bool IsCreatureVisible()
    {
        if (Camera.main == null) return true;

        Vector3 vp = Camera.main.WorldToViewportPoint(_head.transform.position);

        return vp.z > 0 && vp.x >= -1f && vp.x <= 2f && vp.y >= -1f && vp.y <= 2f;
    }

    IEnumerator CO_Sprint()
    {
        if (PointSpawner.Inst == null)
            yield break;

        while (true)
        {
            // _wasSprinting 체크 추가 (실제로 스프린트 중일 때만)
            if (!isDead && !AISpawner.IsInitialSpawn && _wasSprinting)
            {
                AddPoints(-5);

                if (Ducks.Count > 0)
                {
                    Transform tail = Ducks[Ducks.Count - 1];
                    Vector3 spawnPosition = tail.position - tail.forward * 2;
                    PointSpawner.Inst.CreatePoint(spawnPosition, Random.Range(2f, 4f), (int)_color);
                }
            }
            yield return Define.WAIT_HALFOFHALF_SEC;
        }
    }


    float comboTimer = 0f;
    int currentCombo = 0;
    float comboDuration = 1;
    int maxCombo = 8; // 최대 콤보 수
    bool isComboActive = false; // 콤보 활성화 여부

    // 마지막으로 콤보 사운드를 재생한 프레임(같은 프레임에 여러 번 재생되는 것을 막기 위함)
    int _lastComboSoundFrame = -1;


    public void AddPoints(int delta)
    {
        int before = points;

        points += delta;
        if (points < startingPoints)
            points = startingPoints;

        if (!_head.IsPlayer)
            return;

        if (points != before)
            OnPointsChanged?.Invoke(points);


        if (delta > 0)
        {
            ButtWiggleAnim();
            // 콤보가 활성화되지 않았으면 활성화
            if (!isComboActive)
            {
                isComboActive = true;
            }

            // 콤보 타이머 초기화
            comboTimer = 0f;  

            // 콤보 증가: maxCombo 도달 이후 다음 획득 시 다시 1부터 시작하도록 래핑
     
            if(currentCombo >= maxCombo)
            {
              
                currentCombo = 1;
            }
            else if(currentCombo == 7)
            {
                ShowComboNoti();
                currentCombo = currentCombo + 1;
            }
            else
            {
                currentCombo = currentCombo + 1;
            }

            // 같은 프레임에서 여러 번 소리가 재생되지 않도록 프레임 단위로 제한
            if (_lastComboSoundFrame != Time.frameCount)
            {
                Managers.Sound.PlayComboSound(currentCombo);
                _lastComboSoundFrame = Time.frameCount;
            }
        }

    }


    #region Casting
    private static readonly Vector3 _buttScale1 = new Vector3(1.25f, 0.85f, 1f);
    private static readonly Vector3 _buttScale2 = new Vector3(0.85f, 1.25f, 1f);
    private static readonly Vector3 _buttScale3 = new Vector3(1.15f, 0.92f, 1f);
    private static readonly Vector3 _buttScale4 = new Vector3(0.92f, 1.15f, 1f);
    private static readonly Vector3 _buttScale5 = new Vector3(1.05f, 0.97f, 1f);
    private static readonly Vector3 _buttScaleOriginal = Vector3.one;
    #endregion
    void ButtWiggleAnim()
    {
        if (_bodySr == null) return;
        _bodySr.transform.DOKill();

        Sequence wiggleSeq = DOTween.Sequence();
        wiggleSeq.AppendInterval(0.2f);

        // X, Y축 번갈아가며 변화 (젤리 느낌)
        wiggleSeq.Append(_bodySr.transform.DOScale(_buttScale1, 0.1f)
            .SetEase(Ease.OutBack));
        wiggleSeq.Append(_bodySr.transform.DOScale(_buttScale2, 0.1f)
            .SetEase(Ease.InOutQuad));
        wiggleSeq.Append(_bodySr.transform.DOScale(_buttScale3, 0.08f)
            .SetEase(Ease.InOutQuad));
        wiggleSeq.Append(_bodySr.transform.DOScale(_buttScale4, 0.08f)
            .SetEase(Ease.InOutQuad));
        wiggleSeq.Append(_bodySr.transform.DOScale(_buttScale5, 0.06f)
            .SetEase(Ease.InOutQuad));
        wiggleSeq.Append(_bodySr.transform.DOScale(_buttScaleOriginal, 0.06f)
            .SetEase(Ease.InOutBack));

        wiggleSeq.SetUpdate(true);
    }
    void CreatureSize()
    {
        if (points < startingPoints)
            points = startingPoints;

        int baseTailCount = 5;
        int extraParts = (points - startingPoints) / pieceForPoints;
        if (extraParts < 0)
            extraParts = 0;

        int snakeParts = baseTailCount + extraParts;

        if (Ducks.Count < snakeParts)
            AddDuckling();
        else if (Ducks.Count > snakeParts)
            RemoveDuckling();
    }


    void CreatureScale()
    {
        float scale = (float)points / pointsForScale;
        scale = 1 + scale * scaleOffset;
        referenceScale = Mathf.Lerp(referenceScale, scale, Time.deltaTime * 1);
    }



    void CreatureSprint()
    {
        bool sprintInput = false;

        if (_head.IsPlayer)
            sprintInput = CnInputManager.GetButton("Jump");
        else if (_ai != null)
            sprintInput = _ai.sprint;

        bool sprintNow = sprintInput;

        if (points <= 500)
        {
            sprintNow = false;

            if (_head.IsPlayer)
            {
                _uigamescene._skillUI.ForceSprintOff();
            }

            if (_wasSprinting)
                OnSprintEnd();
        }

        if (sprintNow && !_wasSprinting)
        {
            OnSprintStart();
            OnSprintChanged?.Invoke(true);
        }


        if (!sprintNow && _wasSprinting)
        {
            OnSprintEnd();
            OnSprintChanged?.Invoke(false);
        }


        float sprintBonus = sprintNow ? (originalSprintMultiplier - 1f) : 0f;
        float itemBonus = _ActiveSpeedBuff ? (_speedBuffValue - 1f) : 0f;
        speedMultiplier = 1f + sprintBonus + itemBonus;

        UpdateWaveEmission();

        _wasSprinting = sprintNow;
    }

    void OnSprintStart()
    {
       
        _sprintParticle.gameObject.SetActive(true);
    }

    void OnSprintEnd()
    {
        _sprintParticle.gameObject.SetActive(false);
    }

    void UpdateWaveEmission()
    {
        if (_waveParitcle == null) return;

        bool shouldBeFast = (speedMultiplier > 1f);

        if (shouldBeFast && !_isWaveFast)
        {
            _isWaveFast = true;
            var emission = _waveParitcle.emission;
            emission.rateOverTime = 8f;
        }
        else if (!shouldBeFast && _isWaveFast)
        {
            _isWaveFast = false;
            var emission = _waveParitcle.emission;
            emission.rateOverTime = 5f;
        }
    }
    void AddDuckling()
    {
        if (Ducks.Count == 0)
        {
            Ducks.Add(_head.transform);
        }

        Piece newPiece = Managers.Object.Spawn<Piece>(_head.transform.position, "Piece");

        // 맨 뒤에 추가
        Ducks.Add(newPiece.transform);
        _cachedPieces.Add(newPiece);

        newPiece.Init(Ducks.Count - 1, this);

        if (_head.IsPlayer)
        {
            ShowPointNoti(1);
            // 앞에서 뒤로 크기 커졌다 작아지는 애니메이션
            PlayPieceWiggleWave();
        }
    }
  
    void PlayPieceWiggleWave()
    {
        if (Ducks.Count <= 1) return;

        int maxPieces = Mathf.Min(Ducks.Count, 15);
        float delay = 0f;
        float delayStep = 0.025f;

        for (int i = 1; i < maxPieces; i++)
        {
            Transform piece = Ducks[i];
            if (piece == null) continue;

            Piece p = piece.GetComponent<Piece>();
            if (p != null && !p.IsPulsing)
            {
                StartCoroutine(CO_SimplePieceWiggle(piece, delay));
                delay += delayStep;
            }
        }
    }

    // 간단한 크기 변화 애니메이션
    IEnumerator CO_SimplePieceWiggle(Transform piece, float delay)
    {
        Piece p = piece.GetComponent<Piece>();
        if (p == null || p.IsPulsing) yield break;

        p.IsPulsing = true;
        yield return new WaitForSeconds(delay);

        float currentScale = p.Parameters.referenceScale;
        Vector3 baseScale = new Vector3(currentScale, currentScale, currentScale);

        piece.DOKill();

        Vector3 enlarged = baseScale * 1.5f; // 1.3배 커짐

        Sequence scaleSeq = DOTween.Sequence();

        scaleSeq.Append(piece.DOScale(enlarged, 0.2f).SetEase(Ease.OutQuad));
        scaleSeq.Append(piece.DOScale(baseScale, 0.2f).SetEase(Ease.InQuad));

        scaleSeq.OnComplete(() => {
            if (p != null)
            {
                p.IsPulsing = false;
                piece.localScale = baseScale;
            }
        });

        scaleSeq.Play();
    }
    IEnumerator PlayPushBackAnimation()
    {
        int maxAnim = Mathf.Min(Ducks.Count, 5);

        for (int i = 2; i < maxAnim; i++)
        {
            Transform piece = Ducks[i];
            if (piece == null) continue;

            Piece p = piece.GetComponent<Piece>();
            if (p != null && p.IsPulsing) continue;

            if (p != null) p.IsPulsing = true;

            Vector3 original = piece.localScale;
            piece.localScale = original * 0.85f;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.12f;
                piece.localScale = Vector3.Lerp(original * 0.85f, original, t);
                yield return null;
            }

            piece.localScale = original;
            if (p != null) p.IsPulsing = false;
        }
    }

    void RemoveDuckling()
    {
        if (Ducks.Count <= 1)
            return;

        if (_head.IsPlayer)
            ShowPointNoti(-1);

        Transform last = Ducks[Ducks.Count - 1];

        if (last != _head.transform)
        {
            Piece piece = last.GetComponent<Piece>();
            if (piece != null)
            {
                //  캐시에서 제거
                _cachedPieces.Remove(piece);

                Managers.Object.Despawn(piece);
            }
            else
            {
                Destroy(last.gameObject);
            }

            Ducks.RemoveAt(Ducks.Count - 1);
        }
    }

    public void Death()
    {
        if (isDead)
            return;

        if (isPlayer)
        {
            Managers.Sound.Play(ESound.Effect, "KillSound");
            VibrationManager.Inst.StopAllCoroutines();
            _uigamescene.BlindGameUI();

            GameScene scene = Managers.Scene.CurrentScene as GameScene;
            scene._ui_rank.BlindRank();
        }

        isDead = true;

        Managers.Object.UnregisterCreature(this);

        ToggleParticle(false);
        _sprintParticle.SetActive(false);

        bool spawnPointsVisible = false;
        Camera cam = Camera.main;
        if (cam != null && _head != null)
        {
            float sqr = (_head.transform.position - cam.transform.position).sqrMagnitude;
            spawnPointsVisible = sqr <= 500f * 500f;
        }

        if (spawnPointsVisible && PointSpawner.Inst != null)
        {
            for (int i = 0; i < Ducks.Count; i++)
            {
                Transform piece = Ducks[i];
                Vector3 randomCircle = Random.insideUnitSphere * 3;
                randomCircle.y = 0;
                Vector3 spawnPos = piece.position + randomCircle;

                //  isDeathSpawn = true로 전달
                PointSpawner.Inst.CreatePoint(spawnPos, 0f, -1, true);
            }
        }

        if (AISpawner.Inst != null)
            AISpawner.Inst.EnqueueCreatureSpawn(Random.Range(500, 1000));

        StartCoroutine(FadeToDeathRoutine());
    }

    IEnumerator FadeToDeathRoutine()
    {
        float lerper = 1f;
        float lerpTime = 0.5f;

        Camera cam = Camera.main;
        bool visible = false;
        if (cam != null && _head != null)
        {
            float sqr = (_head.transform.position - cam.transform.position).sqrMagnitude;
            visible = sqr <= 500f * 500f;
        }

        Piece[] pieces = null;
        SpriteRenderer[] pieceSRs = null;
        SpriteRenderer headSR = null;

        if (visible)
        {
            int count = Ducks.Count;
            pieces = new Piece[count];
            pieceSRs = new SpriteRenderer[count];
            for (int i = 0; i < count; i++)
            {
                Transform t = Ducks[i];
                if (t == null) continue;
                Piece p = t.GetComponent<Piece>();
                pieces[i] = p;
                if (p != null)
                    pieceSRs[i] = p.spriteRenderer;
            }

            headSR = (_head != null) ? _head._sr : null;

            //  프레임 카운터 추가
            int frameCount = 0;

            while (lerper >= 0f)
            {
                lerper -= Time.deltaTime / lerpTime;
                float a = Mathf.Clamp01(lerper);

                //  2프레임마다만 색상 업데이트
                if (frameCount % 1 == 0)
                {
                    for (int i = 0; i < pieces.Length; i++)
                    {
                        SpriteRenderer sr = pieceSRs[i];
                        if (sr != null)
                        {
                            Color spritecol = sr.color;
                            spritecol.a = a;
                            sr.color = spritecol;
                        }

                        Piece pieceParams = pieces[i];
                        if (pieceParams != null)
                        {
                            if (pieceParams._glow != null)
                            {
                                Color glowCol = pieceParams._glow.color;
                                glowCol.a = 0f;
                                pieceParams._glow.color = glowCol;
                            }

                            pieceParams.gameObject.tag = "Untagged";
                            pieceParams.TurnOffShadow();
                       }
                    }

                    if (headSR != null)
                    {
                        Color spriteHeadCol = headSR.color;
                        spriteHeadCol.a = a;
                        headSR.color = spriteHeadCol;
                        _head.gameObject.tag = "Untagged";
                    }
                }

                frameCount++;
                yield return null;
            }
        }



        if (_head.IsPlayer)
        {

            GameScene gameScene = Managers.Scene.CurrentScene as GameScene;
            gameScene.cameraController._Joystick.gameObject.SetActive(false);


            _DeathAnim.SetActive(true);
            StartCoroutine(RotateFadeOut(_bodySr, 1.5f));
            yield return StartCoroutine(gameScene.cameraController.Co_ZoomToFixedSize(20, 1.5f));
    
            yield return WAIT_1_SEC;

            UI_GameOver uiGameover = Managers.UI.ShowPopupUI<UI_GameOver>();
            uiGameover.SetInfo(points);


        }



        if (pieces != null)
        {
            for (int i = 1; i < pieces.Length; i++)
            {
                Piece piece = pieces[i];
                if (piece != null)
                    Managers.Object.Despawn(piece);
            }
        }
        else
        {
            for (int i = 1; i < Ducks.Count; i++)
            {
                Transform t = Ducks[i];
                Piece piece = t.GetComponent<Piece>();
                if (piece != null)
                    Managers.Object.Despawn(piece);
            }
        }

        Ducks.Clear();
        _cachedPieces.Clear();  //  캐시도 클리어

        Managers.Pool.Push(gameObject);
    }

    #region Colors
    Color _defaultGlow = new Color(1f, 1f, 1f, 0f);
    Color _itemGlow = new Color(0.7843f, 0.3921f, 0.3921f, 0.7f);
    Color _sprintGlow = new Color(1f, 1f, 1f, 0.7f);
    Color _itemSprintGlow = new Color(0f, 1f, 1f, 0.7f);
    #endregion
    public void ControlGlow(SpriteRenderer glow)
    {
        if (glow == null || isDead)
            return;

        float speed = speedMultiplier;
        Color target;

        if (speed == 1f)
            target = _defaultGlow;
        else if (speed == _speedBuffValue && !_wasSprinting)
            target = _itemGlow;
        else if (speed == originalSprintMultiplier && !_ActiveSpeedBuff)
            target = _sprintGlow;
        else if (speed == originalSprintMultiplier * _speedBuffValue)
            target = _itemSprintGlow;
        else
            target = _defaultGlow;

        glow.color = Color.Lerp(glow.color, target, Time.deltaTime * 8f);
    }


    void ToggleParticle(bool value)
    {
        if (_shadowObject != null)
            _shadowObject.SetActive(value);

        if (_bodyshadowObject != null)
            _bodyshadowObject.SetActive(value);

        if (_waveParitcle != null)
            _waveParitcle.gameObject.SetActive(value);

        if (_ShieldParticle != null)
            _ShieldParticle.SetActive(value);

        if (_buffParticle != null)
            _buffParticle.SetActive(value);

        //if (_sprintParticle != null)
        //    _sprintParticle.SetActive(value);

    }

    public void PlayAddPieceWave()
    {
        StartCoroutine(CO_AddPieceWave());
    }

    IEnumerator CO_AddPieceWave()
    {
        float delay = 0f;
        float delayStep = 0.03f;

        //  최대 20개로 제한
        int maxPieces = Mathf.Min(Ducks.Count, 20);

        for (int i = 0; i < maxPieces; i++)
        {
            Transform tr = Ducks[i];
            StartCoroutine(CO_Pulse(tr, delay));
            delay += delayStep;
        }
        yield break;
    }


    IEnumerator CO_Pulse(Transform tr, float delay)
    {
        Piece piece = tr.GetComponent<Piece>();
        if (piece == null)
            yield break;

        if (piece.IsPulsing)
            yield break;

        piece.IsPulsing = true;

        yield return new WaitForSeconds(delay);

        Vector3 original = tr.localScale;
        Vector3 enlarged = original * 1.35f;

        float t = 0f;
        float time = 0.12f;

        while (t < 1f)
        {
            t += Time.deltaTime / time;
            tr.localScale = Vector3.Lerp(original, enlarged, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            tr.localScale = Vector3.Lerp(enlarged, original, t);
            yield return null;
        }

        tr.localScale = original;

        piece.IsPulsing = false;
    }
    void ShowPointNoti(int value)
    {
        if (_head == null || !_head.IsPlayer)
            return;


        Vector3 dir = _head.transform.forward;
        dir.y = 0;
        dir.Normalize();

        Vector3 basePos = _head.transform.position;

        Vector3 spawnDir;

        if (Mathf.Abs(dir.z) > Mathf.Abs(dir.x))
        {
            float leftOrRight = (Random.value < 0.5f) ? -1f : 1f;
            spawnDir = new Vector3(leftOrRight, 0, 0);
        }
        else
        {
            float upOrDown = (Random.value < 0.5f) ? -1f : 1f;
            spawnDir = new Vector3(0, 0, upOrDown);
        }

        Vector3 offset = spawnDir * 4f;

        UI_Noti noti = Managers.Object.ShowSign(basePos + offset, value);
    }
    void ShowComboNoti()
    {
        if (_head == null || !_head.IsPlayer)
            return;


        Vector3 dir = _head.transform.forward;
        dir.y = 0;
        dir.Normalize();

        Vector3 basePos = _head.transform.position;

        Vector3 spawnDir;

        if (Mathf.Abs(dir.z) > Mathf.Abs(dir.x))
        {
            float leftOrRight = (Random.value < 0.5f) ? -1f : 1f;
            spawnDir = new Vector3(leftOrRight, 0, 0);
        }
        else
        {
            float upOrDown = (Random.value < 0.5f) ? -1f : 1f;
            spawnDir = new Vector3(0, 0, upOrDown);
        }

        Vector3 offset = spawnDir * 4f;

         Managers.Object.ShowCombo(basePos + offset);
    }
    #region Item

    public void ApplyItemBuff(EItemType type, float value, float duration)
    {
        switch (type)
        {
            case EItemType.Speed:

                if (_head.IsPlayer)
                {
                    Managers.Sound.Play(ESound.Effect, "SprintSound");
                    VibrationManager.Inst.VibrateCombo();

                }


                ApplySpeedBuff(value, duration);
                break;

            case EItemType.Shield:

                if (_head.IsPlayer)
                {
                    Managers.Sound.Play(ESound.Effect, "ShieldSound");
                    VibrationManager.Inst.VibrateCombo();
                }



                ApplyShieldBuff(duration);

                break;

            case EItemType.Magnet:

                if (_head.IsPlayer)
                {
                    Managers.Sound.Play(ESound.Effect, "MagnetSound");
                    VibrationManager.Inst.VibrateCombo();
                    Managers.Object.MagnetCollect(_head, 100);
                }
                else
                {
                    Managers.Object.MagnetCollect(_head, 50);
                }
                break;
            default:
                break;
        }
    }

    public bool _activeShield = false;
    Coroutine _shieldCo;
    public bool IsShield => _activeShield;
    public void ApplyShieldBuff(float duration)
    {
        if (_shieldCo != null)
            StopCoroutine(_shieldCo);
        if (_head.IsPlayer)
            Managers.Game.OnShieldBuff?.Invoke(EItemType.Shield, -1, duration);


        _activeShield = true;
        _shieldCo = StartCoroutine(ShieldRoutine(duration));

        _ShieldParticle.SetActive(true);


    }

    IEnumerator ShieldRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        _activeShield = false;
        _ShieldParticle.SetActive(false);
        VibrationManager.Inst.StopAllCoroutines();

        if (_head.IsPlayer)
            Managers.Sound.Play(ESound.Effect, "SkillEndSound");
    }


    Coroutine _speedBuffCo;
    public void ApplySpeedBuff(float buffMultiplier, float duration)
    {
        if (_speedBuffCo != null)
            StopCoroutine(_speedBuffCo);

        _ActiveSpeedBuff = true;
        _SpeedParticle.SetActive(true);

        if (_head.IsPlayer)
            Managers.Game.OnSpeedBuff?.Invoke(EItemType.Speed, buffMultiplier, duration);

        _speedBuffCo = StartCoroutine(SpeedBuffRoutine(buffMultiplier, duration));
    }

    IEnumerator SpeedBuffRoutine(float buffMultiplier, float duration)
    {
        _speedBuffValue = buffMultiplier;

        yield return new WaitForSeconds(duration);

        _speedBuffValue = 1f;
        _speedBuffCo = null;

        _SpeedParticle.SetActive(false);
        _ActiveSpeedBuff = false;
        VibrationManager.Inst.StopAllCoroutines();
        if (_head.IsPlayer)
            Managers.Sound.Play(ESound.Effect, "SkillEndSound");


    }
    IEnumerator RotateFadeOut(SpriteRenderer sr, float duration)
    {
        // 현재 rotation을 저장
        Vector3 currentRot = sr.transform.localEulerAngles;

        // Fade
        sr.DOFade(0f, duration).SetEase(Ease.InQuad);

        // Z축으로만 회전 (X, Y는 현재 값 유지)
        sr.transform.DOLocalRotate(
            new Vector3(currentRot.x, currentRot.y, currentRot.z + 360f),
            duration,
            RotateMode.FastBeyond360
        ).SetEase(Ease.Linear);

        yield return null;
    }



    #endregion




}