using Data;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class UI_GameScene : UI_Scene
{
    public UI_Joystick _joystickUI;
    public UI_Skill _skillUI;

    public Camera mainCamera;
    Dictionary<EItemType, Vector2> _originalPos = new Dictionary<EItemType, Vector2>();
    Dictionary<EItemType, Coroutine> _buffCoroutines = new Dictionary<EItemType, Coroutine>();

    private RectTransform _killRect;
    private TMP_Text _pointText;
    private TMP_Text _killText;
    private TMP_Text _speedTimeText;
    private TMP_Text _shieldTimeText;
    ParticleSystem _killParticle;

    // ✅ Canvas Group 추가
    private CanvasGroup _killLogCanvasGroup;

    #region Enum
    enum GameObjects
    {
        KillObject,
        SpeedItemObject,
        ShieldItemObject,
        ContentObject,
        KillLogObject,
        KillParticle
    }

    enum Buttons { }

    enum Texts
    {
        KillText,
        SpeedTimeText,
        ShieldTimeText,
        KillLogText,
        DeadLogText
    }

    enum Images
    {
        KillImage
    }
    #endregion

    public override bool Init()
    {
        if (!base.Init())
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        _joystickUI = FindObjectOfType<UI_Joystick>();
        _skillUI = FindObjectOfType<UI_Skill>();
        mainCamera = Camera.main;

        GetObject((int)GameObjects.SpeedItemObject).SetActive(false);
        GetObject((int)GameObjects.ShieldItemObject).SetActive(false);

        _originalPos[EItemType.Speed] =
        GetObject((int)GameObjects.SpeedItemObject).GetComponent<RectTransform>().anchoredPosition;

        _originalPos[EItemType.Shield] =
            GetObject((int)GameObjects.ShieldItemObject).GetComponent<RectTransform>().anchoredPosition;

        _killText = GetText((int)Texts.KillText);

        _speedTimeText = GetText((int)Texts.SpeedTimeText);
        _shieldTimeText = GetText((int)Texts.ShieldTimeText);

        _killRect = GetObject((int)GameObjects.KillObject).GetComponent<RectTransform>();

        if (mainCamera != null)
        {
            var killImage = GetImage((int)Images.KillImage);
            if (killImage != null)
            {
                Vector3 screenPos = killImage.transform.position;
                Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
                Managers.Game.BodyDestination = targetWorldPos;
            }
        }

        _killParticle = GetObject((int)GameObjects.KillParticle).GetComponent<ParticleSystem>();

        // ✅ Canvas Group 가져오기
        _killLogCanvasGroup = GetObject((int)GameObjects.KillLogObject).GetComponent<CanvasGroup>();

        GetObject((int)GameObjects.KillLogObject).SetActive(false);
        return true;
    }

    private void Awake()
    {
        Init();
        Managers.Object.Player.OnPlayerMove += OnPlayerMove;
    }

    void Start()
    {
        Util.PlayUIEnter(GetObject((int)GameObjects.KillObject), new Vector2(-500, 0), 3);
    }

    private void OnEnable()
    {
        Managers.Game.OnKill += OnChangedKillPoint;
        Managers.Game.OnSpeedBuff += HandleBuffUI;
        Managers.Game.OnShieldBuff += HandleBuffUI;
    }

    private void OnDisable()
    {
        Managers.Game.OnKill -= OnChangedKillPoint;
        Managers.Game.OnShieldBuff -= HandleBuffUI;
        Managers.Game.OnSpeedBuff -= HandleBuffUI;
    }

    private void OnDestroy()
    {
        if (Managers.Object.Player != null)
        {
            Managers.Object.Player.OnPlayerMove -= OnPlayerMove;
        }
    }

    void Update()
    {
        if (!_joystickUI.gameObject.activeInHierarchy)
            return;
    }

    private Vector3 _cachedAdjustedPos;

    public void OnPlayerMove()
    {
        if (_killRect == null || mainCamera == null)
            return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(_killRect.position);

        _cachedAdjustedPos.x = worldPos.x;
        _cachedAdjustedPos.y = 0f;
        _cachedAdjustedPos.z = worldPos.z;

        Managers.Game.BodyDestination = _cachedAdjustedPos;
    }

    void OnChangedKillPoint()
    {
        if (_killText != null)
            _killText.text = Managers.Game.Kill.ToString();
    }

    void HandleBuffUI(EItemType type, float multiplier, float duration)
    {
        if (_buffCoroutines.TryGetValue(type, out Coroutine co))
        {
            StopCoroutine(co);
        }

        if (!GetObjectUI(type).activeSelf)
            ShowItemUI(type);

        _buffCoroutines[type] = StartCoroutine(CO_HideItemOptimized(type, duration));
    }

    IEnumerator CO_HideItemOptimized(EItemType type, float duration)
    {
        float remain = duration;
        TMP_Text timeText = null;

        if (type == EItemType.Speed)
            timeText = _speedTimeText;
        else if (type == EItemType.Shield)
            timeText = _shieldTimeText;

        if (timeText != null)
        {
            timeText.color = Color.white;
            timeText.transform.localScale = Vector3.one;

            timeText.transform.DOKill();
            timeText.transform.localScale = Vector3.one * 1.3f;
            timeText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        int lastDisplayedTime = -1;
        float blinkTimer = 0f;
        const float blinkInterval = 0.12f;
        Color lastColor = Color.white;

        while (remain > 0f)
        {
            float dt = Time.deltaTime;
            remain -= dt;
            int time = Mathf.CeilToInt(remain);

            if (timeText != null)
            {
                if (time != lastDisplayedTime)
                {
                    timeText.text = time.ToString();
                    lastDisplayedTime = time;
                }

                if (time <= 3)
                {
                    blinkTimer += dt;
                    if (blinkTimer >= blinkInterval)
                    {
                        blinkTimer = 0f;
                        float t = Mathf.PingPong(Time.time * 6f, 1f);
                        Color newColor = Color.Lerp(Color.white, Color.red, t);
                        if (newColor != lastColor)
                        {
                            timeText.color = newColor;
                            lastColor = newColor;
                        }
                    }
                }
                else
                {
                    if (lastColor != Color.white)
                    {
                        timeText.color = Color.white;
                        lastColor = Color.white;
                    }
                }
            }

            yield return null;
        }

        HideItemUI(type);

        if (_buffCoroutines.ContainsKey(type))
            _buffCoroutines.Remove(type);
    }

    Vector2 _ItemVec = new Vector2(0, -500);

    public void ShowItemUI(EItemType item)
    {
        GameObject obj = null;

        switch (item)
        {
            case EItemType.Speed:
                obj = GetObject((int)GameObjects.SpeedItemObject);
                break;
            case EItemType.Shield:
                obj = GetObject((int)GameObjects.ShieldItemObject);
                break;
        }

        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.anchoredPosition = _originalPos[item];

        obj.SetActive(true);

        rt.anchoredPosition += _ItemVec;

        rt.DOAnchorPos(_originalPos[item], 0.7f).SetEase(Ease.OutBack);
    }

    public void HideItemUI(EItemType item)
    {
        GameObject obj = null;

        switch (item)
        {
            case EItemType.Speed:
                obj = GetObject((int)GameObjects.SpeedItemObject);
                break;
            case EItemType.Shield:
                obj = GetObject((int)GameObjects.ShieldItemObject);
                break;
        }

        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.DOAnchorPos(_originalPos[item] + _ItemVec, 0.7f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                rt.anchoredPosition = _originalPos[item];
                obj.SetActive(false);
            });
    }

    GameObject GetObjectUI(EItemType type)
    {
        switch (type)
        {
            case EItemType.Speed:
                return GetObject((int)GameObjects.SpeedItemObject);
            case EItemType.Shield:
                return GetObject((int)GameObjects.ShieldItemObject);
            default:
                return null;
        }
    }

    public void BlindGameUI()
    {
        _skillUI.gameObject.SetActive(false);
        GetObject((int)GameObjects.ContentObject).SetActive(false);
    }

    public void SetKillLog(string kill, string dead)
    {
        Managers.Sound.Play(ESound.Effect, "KillSound");
        GameObject killLogObject = GetObject((int)GameObjects.KillLogObject);

        //  DOTween Kill (기존 애니메이션 중단)
        if (_killLogCanvasGroup != null)
            _killLogCanvasGroup.DOKill();

        if (killLogObject.activeSelf)
        {
            StartCoroutine(HideAndShowKillLog(kill, dead));
        }
        else
        {
            GetText((int)Texts.KillLogText).text = dead;
            GetText((int)Texts.DeadLogText).text = kill;

            //  Alpha 1로 설정
            if (_killLogCanvasGroup != null)
                _killLogCanvasGroup.alpha = 1f;

            killLogObject.SetActive(true);
            PopupOpenAnimation(killLogObject);
            _killParticle.Play();

            StartCoroutine(HideKillLogAfterDelay());
        }
    }

    private IEnumerator HideAndShowKillLog(string kill, string dead)
    {
        //  Fade Out
        if (_killLogCanvasGroup != null)
        {
            _killLogCanvasGroup.DOFade(0f, 0.5f)
                .OnComplete(() =>
                {
                    GetObject((int)GameObjects.KillLogObject).SetActive(false);
                });
        }
        else
        {
            GetObject((int)GameObjects.KillLogObject).SetActive(false);
        }

        yield return new WaitForSeconds(0.6f);

        GetText((int)Texts.KillLogText).text = dead;
        GetText((int)Texts.DeadLogText).text = kill;

        //  Alpha 1로 설정
        if (_killLogCanvasGroup != null)
            _killLogCanvasGroup.alpha = 1f;

        GetObject((int)GameObjects.KillLogObject).SetActive(true);
        PopupOpenAnimation(GetObject((int)GameObjects.KillLogObject));
        _killParticle.Play();

        StartCoroutine(HideKillLogAfterDelay());
    }

    //  Fade Out으로 변경
    private IEnumerator HideKillLogAfterDelay()
    {
        yield return WAIT_2_SEC;

        //  2초 동안 Fade Out
        if (_killLogCanvasGroup != null)
        {
            _killLogCanvasGroup.DOFade(0f, 1f)
                .OnComplete(() =>
                {
                    GetObject((int)GameObjects.KillLogObject).SetActive(false);
                });
        }
        else
        {
            GetObject((int)GameObjects.KillLogObject).SetActive(false);
        }
    }
}