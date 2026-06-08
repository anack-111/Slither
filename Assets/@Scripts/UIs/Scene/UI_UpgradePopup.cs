using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_UpgradePopup : UI_Popup
{
    #region Enum
    enum GameObjects
    {
        ContentObject
    }

    enum Buttons
    {

    }

    enum Texts
    {

    }

    enum Images
    {
       // BackgroundImage
    }

    #endregion

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {

     
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


        Refresh();
        return true;
    }

    void Refresh()
    {


    }

}
