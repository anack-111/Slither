using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerInfoPopup : UI_Popup
{
    #region Enum
    enum GameObjects 
    {
        Background,
        ContentObject,
        ShopObject
    }
    enum Buttons
    {
      //  CloseButton
    }

    enum Texts 
    {
        BodyCountValue,
        MaxPointValue,
        MaxKillPointValue,
        MaxPlayTimeValueText
    }
    enum Images 
    {
        PlayerImage,
        ACImage
    }
    #endregion

    public Action OnCloseButton;
    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Refresh();
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }

    private void Refresh()
    {
        string spriteName = Managers.Game.PlayerSpriteNames[0].Replace("_Head", "");
        GetImage((int)Images.PlayerImage).sprite = Managers.Resource.Load<Sprite>(spriteName);

        GetText((int)Texts.BodyCountValue).text = Managers.Game.BodyCount.ToString();
        GetText((int)Texts.MaxPointValue).text = Managers.Game.Point.ToString();
        GetText((int)Texts.MaxKillPointValue).text = Managers.Game.MaxKill.ToString();
        GetText((int)Texts.MaxPlayTimeValueText).text = Util.FormatTime(Managers.Game.MaxPlayTime);
        int equippedID = Managers.Game.EquippedAccessoryIndex;
        AccessoryData data = Managers.Data.AccessoryDic[equippedID];
        GetImage((int)Images.ACImage).sprite = Managers.Resource.Load<Sprite>(data.SpriteName);
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));


       // GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetObject((int)GameObjects.Background).gameObject.BindEvent(OnClickCloseButton);

        GetObject((int)GameObjects.ShopObject).gameObject.GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.ShopObject).BindEvent(OnClickStoreButton);


        return true;
    }
    private void OnClickStoreButton()
    {
        Managers.UI.ShowToast("Coming Soon!");
        Managers.Sound.Play(Define.ESound.Effect, "Button_Toast");
    }


    private void OnClickCloseButton()
    {
        OnCloseButton?.Invoke();
        //¿œ¥‹ ¥›±‚
        gameObject.SetActive(false);
        Managers.Sound.Play(Define.ESound.Effect, "BackButton");
    }
}
