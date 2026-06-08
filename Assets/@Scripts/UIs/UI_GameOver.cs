using Data;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UI_GameOver : UI_Popup
{

    int _point;
    #region Enum
    enum GameObjects
    {
        ContentObject
    }
    enum Buttons
    {
        CloseButton,
        ADCloseButton
    }
    enum Texts
    {
        RankTextValue,
        BodyTextValue,
        PointTextValue,
        KillTextValue,
        LifeTimeTextValue,
        ButtonKillPointValue,
        ADButtonKillPointValue
    }

    enum Images
    {
        PlayerImage,
        ACImage
    }
    #endregion

 
    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Managers.Sound.Play(Define.ESound.Effect, "PopupSound");
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }
    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));


        GetButton((int)Buttons.CloseButton).gameObject.GetOrAddComponent<UI_ButtonAnimation>();
        GetButton((int)Buttons.ADCloseButton).gameObject.GetOrAddComponent<UI_ButtonAnimation>();

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.ADCloseButton).gameObject.BindEvent(OnClickADCloseButton);

        


        return true;
    }

    public void SetInfo(int point)
    {
        _point = point;


        //일단 여기에 
        if (_point > Managers.Game.Point)
            Managers.Game.Point = _point;

        _kill = Managers.Game.Kill;

        if(_kill == 0)
            _kill = 1;

        Refresh();
    }


    int _kill;
    public void Refresh()
    {
        //플레이어 이미지

        string spriteName = Managers.Game.PlayerSpriteNames[0].Replace("_Head", "");

        GetImage((int)Images.PlayerImage).sprite = Managers.Resource.Load<Sprite>(spriteName);

        int equippedID = Managers.Game.EquippedAccessoryIndex;
        AccessoryData data = Managers.Data.AccessoryDic[equippedID];
        GetImage((int)Images.ACImage).sprite = Managers.Resource.Load<Sprite>(data.SpriteName);

        //랭크

        //UI_Rank scene = Managers.UI.SceneUI as UI_Rank;
        GetText((int)Texts.RankTextValue).text = Managers.Game.PlayerRank.ToString();

        int bodytext = Managers.Game.BodyCount - Managers.Game.Kill;
        GetText((int)Texts.BodyTextValue).text = bodytext.ToString();


        GetText((int)Texts.PointTextValue).text = _point.ToString();
        GetText((int)Texts.KillTextValue).text = Managers.Game.Kill.ToString();

        GameScene gameScene = Managers.Scene.CurrentScene as GameScene;

        if (gameScene != null)
        {
            GetText((int)Texts.LifeTimeTextValue).text = Util.FormatTime(gameScene.playTime);
        }

        GetText((int)Texts.ButtonKillPointValue).text = _kill.ToString();
        GetText((int)Texts.ADButtonKillPointValue).text = (_kill * 2).ToString();

    }


    private bool _clickedClose = false;
    private void OnClickCloseButton()
    {
        if (_clickedClose)
            return;   // 이미 눌렸으면 무시

        Managers.Sound.PlayButtonClick();

        _clickedClose = true; 

        Managers.Game.OnGameOver();
    }
    private void OnClickADCloseButton()
    {
        if (_clickedClose)
            return;   // 이미 눌렸으면 무시

        Managers.Sound.PlayButtonClick();

        _clickedClose = true;

        Managers.Game.BodyCount += Managers.Game.Kill;


        Managers.Game.OnGameOver();
    }

   
}
