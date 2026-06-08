using Data;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using Sequence = DG.Tweening.Sequence;
using Toggle = UnityEngine.UI.Toggle;

public class UI_LobbyScene : UI_Scene
{

    #region Enum
    enum GameObjects
    {
        ContentObject,
        InputField,
        PlayerObject,
        TopGroup,
        MiddleGroup,
        ButtomGroup,
        BodyGroup
    }

    enum Buttons
    {
        PlayButton,
        CustomButton,
        SettingButton,
        //BodyButton,
        RankButton,
        QuickMatchButton,
        PrivateMatchButton,
    }

    enum Texts
    {
        TitleText,
        MaxPointValueText,
        BodyTextValueText
    }

    enum Images
    {
        PlayerHeadImage,
        ACImage,
        SettingImage
    }

    #endregion

    TMP_Text _titleText;

    TMP_InputField _nameInput;


    UI_CustomPopup _uiCustomPopup;
    UI_UpgradePopup _uiUpgradePopup;
    UI_PlayerInfoPopup _PlayerInfoPopup;
    UI_SettingPopup _uiSettingPopup;
    UI_RankPopup _uiRankPopup;
    UI_QuickMatchPopup _uiQuickMatchPopup;
    UI_PrivateMatchPopup _uiPrivateMatchPopup;
    UI_PrivateMatchRoomPopup _uiPrivateMatchRoomPopup;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {

       // StartCoroutine(Co_Wave());
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));

    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Object Bind
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        #endregion


        _uiCustomPopup = Managers.UI.ShowPopupUI<UI_CustomPopup>();
        _uiUpgradePopup = Managers.UI.ShowPopupUI<UI_UpgradePopup>();
        _PlayerInfoPopup = Managers.UI.ShowPopupUI<UI_PlayerInfoPopup>();
        _uiSettingPopup = Managers.UI.ShowPopupUI<UI_SettingPopup>();
        _uiRankPopup = Managers.UI.ShowPopupUI<UI_RankPopup>();
        _uiQuickMatchPopup = Managers.UI.ShowPopupUI<UI_QuickMatchPopup>();
        _uiPrivateMatchPopup = Managers.UI.ShowPopupUI<UI_PrivateMatchPopup>();
        _uiPrivateMatchRoomPopup = Managers.UI.ShowPopupUI<UI_PrivateMatchRoomPopup>();


        GetButton((int)Buttons.CustomButton).gameObject.BindEvent(OnClickCustomButton);
        GetButton((int)Buttons.SettingButton).gameObject.BindEvent(OnClickSettingButton);
        GetButton((int)Buttons.QuickMatchButton).gameObject.BindEvent(OnClickQuickMatchButton);
        GetButton((int)Buttons.PrivateMatchButton).gameObject.BindEvent(OnClickPrivateMatchButton);

        _uiCustomPopup.gameObject.SetActive(false);
        _uiUpgradePopup.gameObject.SetActive(false);
        _PlayerInfoPopup.gameObject.SetActive(false);
        _uiSettingPopup.gameObject.SetActive(false);
        _uiRankPopup.gameObject.SetActive(false);
        _uiQuickMatchPopup.gameObject.SetActive(false);
        _uiPrivateMatchPopup.gameObject.SetActive(false);
        _uiPrivateMatchRoomPopup.gameObject.SetActive(false);

        _uiSettingPopup.OnClose += RotateSetting;
        _uiCustomPopup.OnCloseButton += Refresh;


        _titleText = GetText((int)Texts.TitleText);
        _nameInput = GetObject((int)GameObjects.InputField).GetComponent<TMP_InputField>();


        GetButton((int)Buttons.PlayButton).gameObject.BindEvent(OnClickPlayButton);
        GetText((int)Texts.MaxPointValueText).text = Managers.Game.Point.ToString();


        GetButton((int)Buttons.CustomButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.SettingButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.RankButton).GetOrAddComponent<UI_ButtonAnimation>();

        GetObject(((int)GameObjects.BodyGroup)).GetOrAddComponent<UI_ButtonAnimation>();

        GetObject(((int)GameObjects.BodyGroup)).BindEvent(OnClickBodyButton);
        GetButton((int)Buttons.RankButton).gameObject.BindEvent(OnClickRankButton);

        GetObject((int)GameObjects.PlayerObject).BindEvent(OnClickPlayerInfoButton);
        //GetObject((int)GameObjects.PlayerObject).GetOrAddComponent<UI_ButtonAnimation>();




        PlayButtonAnimation();

        Refresh();
        return true;
    }

    private void OnClickRankButton()
    {
        Managers.Sound.PlayButtonClick();
        _uiRankPopup.gameObject.SetActive(true);
    }

    void OnClickBodyButton()
    {
        Managers.Sound.Play(Define.ESound.Effect, "Button_Toast");
        Managers.UI.ShowToast("Coming Soon");
    }

    private bool _isAnimating = false; // 애니메이션 진행 중인지 여부 체크

    private bool isSettingOpen = false;

    private void RotateSetting()
    {
        Transform tr = GetImage((int)Images.SettingImage).transform;

        float delta = isSettingOpen ? -50f : 50f;

        tr.DORotate(
            new Vector3(0, 0, delta),
            0.5f,
            RotateMode.LocalAxisAdd
        ).SetEase(Ease.OutBack);

        isSettingOpen = !isSettingOpen;
    }

    private void OnClickPlayerInfoButton()
    {

        if (_isAnimating)
            return;


        _PlayerInfoPopup.OnCloseButton = () =>
        {
            Refresh();
            StartCoroutine(ClosePlayerInfoPopup());
        };


        StartCoroutine(OpenPlayerInfoPopup());

        Managers.Sound.PlayButtonClick();
    }

    private IEnumerator OpenPlayerInfoPopup()
    {

        _isAnimating = true;

 
        Util.ShowAndHideUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, 700), 0.2f);
        Util.ShowAndHideUI(GetObject((int)GameObjects.ButtomGroup), new Vector2(0, -500), 0.2f);
        Util.HideCanvasGroup(GetObject((int)GameObjects.MiddleGroup), 0.2f);


        yield return new WaitForSeconds(0.2f);
  
        _PlayerInfoPopup.gameObject.SetActive(true);

        _isAnimating = false;
    }

    private IEnumerator ClosePlayerInfoPopup()
    {
        // 애니메이션 시작 전에 _isAnimating을 true로 설정
        _isAnimating = true;

        // Player Info Popup을 닫기 위한 애니메이션
        Util.ShowAndHideUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, -700), 0.2f);
        Util.ShowAndHideUI(GetObject((int)GameObjects.ButtomGroup), new Vector2(0, 500), 0.2f);
        Util.ShowCanvasGroup(GetObject((int)GameObjects.MiddleGroup), 0.2f);

        // 0.3초 기다리기
        yield return new WaitForSeconds(0.2f);

        // 애니메이션 후 실제로 비활성화 처리
        _PlayerInfoPopup.gameObject.SetActive(false);

        // 애니메이션 완료 후 _isAnimating을 false로 설정
        _isAnimating = false;
    }

    private void OnClickQuickMatchButton()
    {
        Managers.Sound.PlayButtonClick();
        LobbyMatchMakingManager.Instance.StartQuickMatch();
    }

    private void OnClickPrivateMatchButton()
    {
        Managers.Sound.PlayButtonClick();
        if (_uiPrivateMatchPopup.gameObject.activeSelf)
        {
            _uiPrivateMatchPopup.gameObject.SetActive(false);
            return;
        }
        _uiPrivateMatchPopup.gameObject.SetActive(true);
    }

    private void OnClickSettingButton()
    {
        RotateSetting();
        _uiSettingPopup.gameObject.SetActive(true);
        Managers.Sound.PlayButtonClick();
    }

    private void OnClickCustomButton()
    {

        _uiCustomPopup.OnCloseButton = () =>
        {
            Refresh();
        };
        _uiCustomPopup.gameObject.SetActive(true);
        _uiCustomPopup.OnClickToggleButton(0);
        Managers.Sound.PlayButtonClick();
    }

    void Refresh()
    {
        string spriteName = Managers.Game.PlayerSpriteNames[0].Replace("_Head", "");

        GetImage((int)Images.PlayerHeadImage).sprite = Managers.Resource.Load<Sprite>(spriteName);

        int equippedID = Managers.Game.EquippedAccessoryIndex;
        AccessoryData data = Managers.Data.AccessoryDic[equippedID];
        GetImage((int)Images.ACImage).sprite = Managers.Resource.Load<Sprite>(data.SpriteName);

        GetText((int)Texts.BodyTextValueText).text = Managers.Game.BodyCount.ToString();
    }

    bool _isChangingScene = false;
    private void OnClickPlayButton()
    {

        if (_isChangingScene)
            return;

        PlayerPrefs.SetInt("ISFIRST", 0);

        //  이름 읽어서 GameManager에 저장
        string inputName = _nameInput != null ? _nameInput.text : "";
        if (string.IsNullOrWhiteSpace(inputName))
            inputName = "무명인";            // 기본 이름 원하는 대로

        Managers.Game.PlayerName = inputName;

        _isChangingScene = true;

        Managers.Scene.LoadScene(Define.EScene.GameScene, transform);

        Managers.Sound.PlayButtonClick();

    }


    void Start()
    {

    }


    IEnumerator Co_Wave()
    {
        TMP_TextInfo textInfo = _titleText.textInfo;
        float waveHeight = 15f;     // 올라가는 높이
        float waveSpeed = 2f;       // 애니메이션 속도
        float waveDelay = 0.15f;    // 글자 간 딜레이

        while (true)
        {
            _titleText.ForceMeshUpdate();
            textInfo = _titleText.textInfo;
            int count = textInfo.characterCount;

            float time = Time.time * waveSpeed;

            for (int i = 0; i < count; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                int matIndex = textInfo.characterInfo[i].materialReferenceIndex;

                Vector3[] verts = textInfo.meshInfo[matIndex].vertices;

                float wave = Mathf.Sin(time + i * waveDelay) * waveHeight;

                verts[vertexIndex + 0].y += wave;
                verts[vertexIndex + 1].y += wave;
                verts[vertexIndex + 2].y += wave;
                verts[vertexIndex + 3].y += wave;
            }

            // 변경된 mesh 반영
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _titleText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null;
        }
    }


    Sequence _pulseSeq;


    void PlayButtonAnimation()
    {
        // 버튼 GameObject 가져오기
        GameObject btnObj = GetButton((int)Buttons.PlayButton).gameObject;

        // RectTransform 가져오기
        RectTransform rect = btnObj.GetComponent<RectTransform>();

        // 기존 DOTween 제거
        if (_pulseSeq != null)
            _pulseSeq.Kill();

        // DOTween 시퀀스 생성
        _pulseSeq = DOTween.Sequence();

        _pulseSeq.Append(rect.DOScale(1.15f, 0.6f).SetEase(Ease.OutQuad))
                 .Append(rect.DOScale(1.0f, 0.6f).SetEase(Ease.InQuad))
                 .AppendInterval(4f)
                 .SetLoops(-1);   // 무한 반복
    }

}
