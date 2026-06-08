using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UI_BattlePopup : UI_Popup
{

    #region Enum
    enum GameObjects
    {
        ContentObject,
        InputField,
    }

    enum Buttons
    {
        PlayButton
    }

    enum Texts
    {
        TitleText,
        MaxPointValueText,
    }

    enum Images
    {

    }

    #endregion

    TMP_Text _titleText;

    TMP_InputField _nameInput;


    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {

        StartCoroutine(Co_Wave());
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


        _titleText = GetText((int)Texts.TitleText);
        _nameInput = GetObject((int)GameObjects.InputField).GetComponent<TMP_InputField>();


        GetButton((int)Buttons.PlayButton).gameObject.BindEvent(OnClickPlayButton);
        GetButton((int)Buttons.PlayButton).GetOrAddComponent<UI_ButtonAnimation>();
        GetText((int)Texts.MaxPointValueText).text = Managers.Game.Point.ToString();



        Refresh();
        return true;
    }

    void Refresh()
    {


    }

    bool _isChangingScene = false;
    private void OnClickPlayButton()
    {

        if (_isChangingScene)
            return;

        //  이름 읽어서 GameManager에 저장
        string inputName = _nameInput != null ? _nameInput.text : "";
        if (string.IsNullOrWhiteSpace(inputName))
            inputName = " ";            // 기본 이름 원하는 대로

        Managers.Game.PlayerName = inputName;

        _isChangingScene = true;

        Managers.Scene.LoadScene(Define.EScene.GameScene, transform);
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
}
