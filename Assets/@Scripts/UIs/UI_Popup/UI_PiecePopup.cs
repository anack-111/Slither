// UI_PiecePopup.cs

using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PiecePopup : UI_Popup
{
    public ScrollRect _scrollRect;
    public Action OnPieceChange;

    #region Enum
    enum GameObjects
    {
        ContentObject,
        TopGroup,
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
    }

    private void Refresh()
    {

        GetText((int)Texts.BodyValueText).text = Managers.Game.BodyCount.ToString();

        GameObject container = GetObject((int)GameObjects.AccessoryGroupObject);
        container.DestroyChilds();

        //  저장된 스프라이트 이름 가져오기
        string savedSpriteName = Managers.Game.PlayerSpriteNames[1];

        foreach (CustomChildData Data in Managers.Data.CustomChildDic.Values)
        {
            UI_PieceSlot item = Managers.UI.MakeSubItem<UI_PieceSlot>(container.transform);
            item.SetInfo(Data.SpriteName, _scrollRect);

            //  저장된 스프라이트와 같으면 눈 뜨기
            if (!string.IsNullOrEmpty(savedSpriteName) && Data.SpriteName == savedSpriteName)
            {
                item.OpenEyes();
                item.SetAsSelected();  //  선택 상태로 설정
            }

            item.OnItemClick = () =>
            {
                OnPieceChange?.Invoke();
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