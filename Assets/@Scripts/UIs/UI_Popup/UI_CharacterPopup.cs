using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class UI_CharacterPopup : UI_Popup
{
    public ScrollRect _scrollRect;
    public Action OnCharacterChange;

    #region Enum
    enum GameObjects
    {
        ContentObject,
        TopGroup,
        //BottomGroup,
        AccessoryGroupObject,

        BodyObject
    }
    enum Buttons
    {
    }

    enum Texts
    {
        BodyValueText
    }
    enum Images
    {

    }
    #endregion


    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Refresh();

        Util.ShowUI(GetObject((int)GameObjects.ContentObject), new Vector2(-500, 0), 0.6f);
        Util.ShowUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, -500), 0.6f);
        // Util.ShowUI(GetObject((int)GameObjects.BottomGroup), new Vector2(0, 500), 0.6f);


    }

    private void Refresh()
    {
        GetText((int)Texts.BodyValueText).text = Managers.Game.BodyCount.ToString();
        GameObject container = GetObject((int)GameObjects.AccessoryGroupObject);
        container.DestroyChilds();

        foreach (CustomData Data in Managers.Data.CustomDic.Values)
        {
            UI_CharacterSlot item = Managers.UI.MakeSubItem<UI_CharacterSlot>(container.transform);
            item.SetInfo(Data.SpriteName, _scrollRect);


            item.OnItemClick = () =>
            {
                OnCharacterChange?.Invoke();


            };
        }
    }

    


    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetObject((int)GameObjects.BodyObject).gameObject.GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.BodyObject).BindEvent(OnClickStoreButton);
        return true;
    }


    private void OnClickStoreButton()
    {
        Managers.UI.ShowToast("Coming Soon!");
        Managers.Sound.Play(Define.ESound.Effect, "Button_Toast");

    }
}
