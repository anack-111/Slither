using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using DG.Tweening;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Sequence = DG.Tweening.Sequence;

public class UI_CustomPopup : UI_Popup
{
    UI_AccessoryPopup _AccessoryPopup;
    UI_CharacterPopup _CharacterPopup;
    UI_PiecePopup _PiecePopup;

    public Action OnCloseButton;


    //  헤엄치는 애니메이션
    private List<DG.Tweening.Sequence> _swimSequences = new List<Sequence>();
    LobbySimpleMover _LobbySimpleMover;


    #region Enum
    enum GameObjects
    {
        CharacterObject,
        ChildObject,
        AccessoryObject,
        ContentObject,
        TopGroupObject,
        CharacterObjectOn,
        ChildObjectOn,
        AccessoryObjectOn,
        
    }

    enum Buttons
    {
        CloseButton,
    }

    enum Images
    {
        Head,
        HeadImage,
        AccessoryImage,
        Child1Image,
        Child2Image,
        Child3Image,
        Child4Image,
        BodyImage,
        Shadow,
        Shadow1,
        Shadow2,
        Shadow3,
        ShadowHead,
        Shadowbody
    }
    #endregion

    private void Awake()
    {
        Init();

    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _AccessoryPopup = Managers.UI.ShowPopupUI<UI_AccessoryPopup>();
        _AccessoryPopup.gameObject.SetActive(false);
        _AccessoryPopup.OnAccessoryChange += Refresh;

        _CharacterPopup = Managers.UI.ShowPopupUI<UI_CharacterPopup>();
        _CharacterPopup.gameObject.SetActive(false);
        _CharacterPopup.OnCharacterChange += Refresh;
        _CharacterPopup.OnCharacterChange += HeadAmim;


        _PiecePopup = Managers.UI.ShowPopupUI<UI_PiecePopup>();
        _PiecePopup.gameObject.SetActive(false);
        _PiecePopup.OnPieceChange += Refresh;
        _PiecePopup.OnPieceChange += PieceAnim;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.CloseButton).GetOrAddComponent<UI_ButtonAnimation>();

        GetObject((int)GameObjects.CharacterObjectOn).SetActive(false);
        GetObject((int)GameObjects.ChildObjectOn).SetActive(false);
        GetObject((int)GameObjects.AccessoryObjectOn).SetActive(false);

        GetObject((int)GameObjects.CharacterObject).gameObject.BindEvent(() =>
        {
            OnClickToggleButton((int)GameObjects.CharacterObject);
        });

        GetObject((int)GameObjects.ChildObject).gameObject.BindEvent(() =>
        {
            OnClickToggleButton((int)GameObjects.ChildObject);
        });

        GetObject((int)GameObjects.AccessoryObject).gameObject.BindEvent(() =>
        {
            OnClickToggleButton((int)GameObjects.AccessoryObject);
        });

        //  헤엄치는 애니메이션 시작
        StartSwimAnimation();

        CacheChildScales();
        CacheHeadScale();

        Refresh();

        _LobbySimpleMover = FindObjectOfType<LobbySimpleMover>(true);

        return true;
    }


    Vector3 _headOriginScale;

    private void HeadAmim()
    {
        RectTransform rt = GetImage((int)Images.HeadImage).GetComponent<RectTransform>();
        if (rt == null) return;

        float scaleUp = 1.2f;
        float upDuration = 0.18f;
        float downDuration = 0.15f;

        rt.DOKill(true);                 // 기존 트윈 종료 + 값 복원
        rt.localScale = _headOriginScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(
            rt.DOScale(_headOriginScale * scaleUp, upDuration)
              .SetEase(Ease.OutBack)
        );
        seq.Append(
            rt.DOScale(_headOriginScale, downDuration)
              .SetEase(Ease.OutSine)
        );

        seq.Play();
    }

    Dictionary<Images, Vector3> _childOriginScales = new Dictionary<Images, Vector3>();
    void CacheHeadScale()
    {
        RectTransform rt = GetImage((int)Images.HeadImage).GetComponent<RectTransform>();
        if (rt != null)
            _headOriginScale = rt.localScale;
    }
    void CacheChildScales()
    {
        Images[] childs =
        {
        Images.Shadow,
        Images.Shadow1,
        Images.Shadow2,
        Images.Shadow3
    };

        foreach (var child in childs)
        {
            RectTransform rt = GetImage((int)child).GetComponent<RectTransform>();
            if (rt == null) continue;

            _childOriginScales[child] = rt.localScale;
        }
    }

    private void PieceAnim()
    {
        Images[] childs =
        {
             Images.Shadow,
             Images.Shadow1,
             Images.Shadow2,
             Images.Shadow3
        };

        float scaleUp = 1.25f;
        float upDuration = 0.2f;
        float downDuration = 0.15f;
        float delayGap = 0.05f;

        foreach (var child in childs)
        {
            if (!_childOriginScales.ContainsKey(child))
                continue;

            RectTransform rt = GetImage((int)child).GetComponent<RectTransform>();
            if (rt == null) continue;

            rt.DOKill(true); //  트윈 강제 종료 + 값 복원
            rt.localScale = _childOriginScales[child];

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(Array.IndexOf(childs, child) * delayGap);
            seq.Append(
                rt.DOScale(_childOriginScales[child] * scaleUp, upDuration)
                  .SetEase(Ease.OutBack)
            );
            seq.Append(
                rt.DOScale(_childOriginScales[child], downDuration)
                  .SetEase(Ease.OutSine)
            );

            seq.Play();
        }
    }


    void StartSwimAnimation()
    {
        var images = new[]
        {
        Images.HeadImage,
        Images.BodyImage,
        Images.Shadow,
        Images.Shadow1,
        Images.Shadow2,
        Images.Shadow3,
        Images.ShadowHead,
        Images.Shadowbody
    };

        float amplitude = 60f;
        float speed = 2.5f;
        float phaseGap = 0.45f;
        float maxRotate = 20f;

        for (int i = 0; i < images.Length; i++)
        {
            RectTransform rt = GetImage((int)images[i]).GetComponent<RectTransform>();
            if (rt == null) continue;

            Vector2 origin = rt.anchoredPosition;
            bool isHeadGroup = (images[i] == Images.HeadImage || images[i] == Images.ShadowHead);
            bool isBodyGroup = (images[i] == Images.BodyImage || images[i] == Images.Shadowbody);

            // phase 계산: Head/Body 그룹은 0, 나머지는 순차
            float phase;
            if (isHeadGroup || isBodyGroup)
                phase = 0f;
            else
                phase = (i - 2) * phaseGap; // Shadow부터 시작이므로 -2

            // BodyImage와 Shadowbody는 amplitude 감소
            float amplitudeMultiplier = isBodyGroup ? 0.95f : 1f;

            // rotateWeight 계산
            float rotateWeight;
            if (images[i] == Images.ShadowHead)
                rotateWeight = Mathf.Lerp(0.5f, 1f, 0 / (float)(images.Length - 1)); // HeadImage와 동일 (i=0)
            else if (images[i] == Images.Shadowbody)
                rotateWeight = Mathf.Lerp(0.5f, 1f, 1 / (float)(images.Length - 1)); // BodyImage와 동일 (i=1)
            else
                rotateWeight = Mathf.Lerp(0.5f, 1f, i / (float)(images.Length - 1));

            DOTween.To(() => 0f, _ =>
            {
                float t = Time.time * speed - phase;
                float sin = Mathf.Sin(t);
                float cos = Mathf.Cos(t);
                float xOffset = sin * amplitude * amplitudeMultiplier;
                float rotationZ = -cos * maxRotate * rotateWeight;

                rt.anchoredPosition = new Vector2(origin.x + xOffset, origin.y);
                rt.localRotation = Quaternion.Euler(0, 0, rotationZ);
            }, 1f, 999f).SetEase(Ease.Linear);
        }
    }
    int _prevNum;

    public void OnClickToggleButton(int enumNumber)
    {

        Managers.Sound.PlayButtonClick();
        _prevNum = enumNumber;

        if (enumNumber == 0)
        {
            GetObject((int)GameObjects.CharacterObjectOn).SetActive(true);
            GetObject((int)GameObjects.ChildObjectOn).SetActive(false);
            GetObject((int)GameObjects.AccessoryObjectOn).SetActive(false);

            _CharacterPopup.gameObject.SetActive(true);
            _AccessoryPopup.gameObject.SetActive(false);
            _PiecePopup.gameObject.SetActive(false);
        }
        else if (enumNumber == 1)
        {
            GetObject((int)GameObjects.CharacterObjectOn).SetActive(false);
            GetObject((int)GameObjects.ChildObjectOn).SetActive(true);
            GetObject((int)GameObjects.AccessoryObjectOn).SetActive(false);

            _CharacterPopup.gameObject.SetActive(false);
            _AccessoryPopup.gameObject.SetActive(false);
            _PiecePopup.gameObject.SetActive(true);
        }
        else
        {
            GetObject((int)GameObjects.CharacterObjectOn).SetActive(false);
            GetObject((int)GameObjects.ChildObjectOn).SetActive(false);
            GetObject((int)GameObjects.AccessoryObjectOn).SetActive(true);

            _CharacterPopup.gameObject.SetActive(false);
            _AccessoryPopup.gameObject.SetActive(true);
            _PiecePopup.gameObject.SetActive(false);
        }
    }

    private void RefreshCharacter(string name)
    {
        GetImage((int)Images.AccessoryImage).sprite = Managers.Resource.Load<Sprite>(Managers.Game.PlayerSpriteNames[0]);
    }

    private void Refresh(string spritename, bool IsGet)
    {
        GetImage((int)Images.AccessoryImage).sprite = Managers.Resource.Load<Sprite>(spritename);
    }

    public void OnEnable()
    {
        //  팝업 열릴 때 애니메이션 재시작
        if (_swimSequences.Count > 0)
        {
            foreach (var seq in _swimSequences)
            {
                seq?.Restart();
            }
        }
    }

    private void OnClickCloseButton()
    {
        // 팝업들을 먼저 닫아서 OnDisable() 실행
        _AccessoryPopup.gameObject.SetActive(false);
        _CharacterPopup.gameObject.SetActive(false);
        _PiecePopup.gameObject.SetActive(false);

        // OnDisable()에서 복구된 후 UI 갱신
        Refresh();
        _LobbySimpleMover.Refresh();

        OnCloseButton?.Invoke();
        gameObject.SetActive(false);

        Managers.Sound.Play(Define.ESound.Effect, "BackButton");
    }

    public void Refresh()
    {
        if (!_init)
            return;
        
 
        GetImage((int)Images.HeadImage).sprite =
            Managers.Resource.Load<Sprite>(Managers.Game.PlayerSpriteNames[0]);

        string spriteName = Managers.Game.PlayerSpriteNames[0].Replace("_Head", "_Body.sprite");
        GetImage((int)Images.BodyImage).sprite = Managers.Resource.Load<Sprite>(spriteName);

        int equippedID = Managers.Game.EquippedAccessoryIndex;
        AccessoryData data = Managers.Data.AccessoryDic[equippedID];
        GetImage((int)Images.AccessoryImage).sprite = Managers.Resource.Load<Sprite>(data.SpriteName);

        GetImage((int)Images.Child1Image).sprite = Managers.Resource.Load<Sprite>(Managers.Game.PlayerSpriteNames[1]);
        GetImage((int)Images.Child2Image).sprite = Managers.Resource.Load<Sprite>(Managers.Game.PlayerSpriteNames[1]);
        GetImage((int)Images.Child3Image).sprite = Managers.Resource.Load<Sprite>(Managers.Game.PlayerSpriteNames[1]);
        GetImage((int)Images.Child4Image).sprite = Managers.Resource.Load<Sprite>(Managers.Game.PlayerSpriteNames[1]);
    }

    private string GetFirstSpriteName(int scrollNumber)
    {
        if (scrollNumber == 1)
            return Managers.Data.CustomDic.Values.OrderBy(cd => cd.CustomIndex).First().SpriteName;

        return Managers.Data.CustomChildDic.Values.OrderBy(cd => cd.CustomIndex).First().SpriteName;
    }

    private void OnDestroy()
    {
        // 애니메이션 정리
        foreach (var seq in _swimSequences)
        {
            seq?.Kill();
        }
        _swimSequences.Clear();
    }
}