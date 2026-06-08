using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // ★ 추가
using UnityEngine.UI;

public class UI_PausePopup : UI_Popup
{
    #region Enum
    enum GameObjects { Background }
    enum Buttons
    {
        LobbyButton,
        RestartButton,
        PlayButton
    }
    enum Texts { }
    enum Images { /* 필요에 따라 이미지 추가 */ }
    #endregion

    Sequence _seq; // 애니메이션 시퀀스

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {

    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        return true;
    }



}
