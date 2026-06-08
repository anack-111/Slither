using Data;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_CustomItem : UI_Base
{


    #region Enum
    enum GameObjects
    {

    }

    enum Buttons
    {
        
    }

    enum Texts
    {

    }

    enum Images
    {
        CustomImage,
        AccessoryImage
    }

    #endregion

    private void Awake()
    {
        Init();
    }

    public void SetInfo(string DataName, bool isHead = false)
    {
        GetImage((int)Images.CustomImage).sprite = Managers.Resource.Load<Sprite>(DataName);

        if(!isHead) //새기한테는 포함 x
            GetImage((int)Images.AccessoryImage).sprite = Managers.Resource.Load<Sprite>("Default");

    }


    public void SetAccessory(string name)
    {
        GetImage((int)Images.AccessoryImage).sprite = Managers.Resource.Load<Sprite>(name);
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

        int equippedID = Managers.Game.EquippedAccessoryIndex;
        AccessoryData data = Managers.Data.AccessoryDic[equippedID];

        SetAccessory(data.SpriteName);

        return true;
    }
}
