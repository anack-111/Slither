using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_SettingPopup : UI_Popup
{

    public Action OnClose;
    #region Enum
    enum GameObjects
    {
        ContentObject,
 
    }
    enum Buttons
    {
        BackgroundButton,
        SoundEffectOffButton,
        SoundEffectOnButton,
        BackgroundSoundOffButton,
        BackgroundSoundOnButton,
        JoystickFixedOffButton,
        JoystickFixedOnButton,
       // CloseButton
    }

    enum Texts
    {
        SettingTlileText,

        SoundEffectText,
        BackgroundSoundText,
    }

    enum Images
    {
        SoundEffectIconImage,
        BackgroundSoundIconImage,
        JoyStickIconImage
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


        GetButton((int)Buttons.BackgroundButton).gameObject.BindEvent(OnClickBackgroundButton);

        GetButton((int)Buttons.SoundEffectOffButton).gameObject.BindEvent(EffectSoundOn);
        GetButton((int)Buttons.SoundEffectOnButton).gameObject.BindEvent(EffectSoundOff);

        GetButton((int)Buttons.BackgroundSoundOffButton).gameObject.BindEvent(BackgroundSoundOn);
        GetButton((int)Buttons.BackgroundSoundOnButton).gameObject.BindEvent(BackgroundSoundOff);


        GetButton((int)Buttons.JoystickFixedOffButton).gameObject.BindEvent(OnCllickJoystickFixed);
        GetButton((int)Buttons.JoystickFixedOnButton).gameObject.BindEvent(OnCllickJoystickNonFixed);

        if (Managers.Game.BGMOn == false)
        {
            BackgroundSoundOff();
        }
        else
        {
            BackgroundSoundOn();
        }

        if (Managers.Game.EffectSoundOn == false)
        {
            EffectSoundOff();
        }
        else
        {
            EffectSoundOn();
        }

        if (Managers.Game.JoystickType == Define.EJoystickType.Fixed)
        {
            OnCllickJoystickFixed();
        }
        else
        {
            OnCllickJoystickNonFixed();
        }


        #endregion

        Refresh();
        return true;
    }

    public void SetInfo()
    {

        Refresh();
    }

    void Refresh()
    {



    }

    void EffectSoundOn()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.EffectSoundOn = true;
        GetButton((int)Buttons.SoundEffectOnButton).gameObject.SetActive(true);
        GetButton((int)Buttons.SoundEffectOffButton).gameObject.SetActive(false);
        GetImage((int)Images.SoundEffectIconImage).sprite = Managers.Resource.Load<Sprite>("Icon_SoundEffect.sprite");

        Managers.Sound.Play(Define.ESound.Effect, "ToggleButton");
       
    }

    void EffectSoundOff()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.EffectSoundOn = false;
        GetButton((int)Buttons.SoundEffectOnButton).gameObject.SetActive(false);
        GetButton((int)Buttons.SoundEffectOffButton).gameObject.SetActive(true);
        Managers.Sound.Play(Define.ESound.Effect, "ToggleButton");
        GetImage((int)Images.SoundEffectIconImage).sprite = Managers.Resource.Load<Sprite>("Icon_SoundEffect_Mute.sprite");
        
    }

    void BackgroundSoundOn()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.BGMOn = true;
        GetButton((int)Buttons.BackgroundSoundOnButton).gameObject.SetActive(true);
        GetButton((int)Buttons.BackgroundSoundOffButton).gameObject.SetActive(false);
        Managers.Sound.Play(Define.ESound.Effect, "ToggleButton");
        GetImage((int)Images.BackgroundSoundIconImage).sprite = Managers.Resource.Load<Sprite>("Icon_BackgroundSound.sprite");
    }

    void BackgroundSoundOff()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.BGMOn = false;
        GetButton((int)Buttons.BackgroundSoundOnButton).gameObject.SetActive(false);
        GetButton((int)Buttons.BackgroundSoundOffButton).gameObject.SetActive(true);
        Managers.Sound.Play(Define.ESound.Effect, "ToggleButton");
        GetImage((int)Images.BackgroundSoundIconImage).sprite = Managers.Resource.Load<Sprite>("Icon_BackgroundSound_Mute.sprite");
    }

    void OnCllickJoystickFixed()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.JoystickType = Define.EJoystickType.Fixed;
        GetButton((int)Buttons.JoystickFixedOnButton).gameObject.SetActive(true);
        GetButton((int)Buttons.JoystickFixedOffButton).gameObject.SetActive(false);
        GetImage((int)Images.JoyStickIconImage).sprite = Managers.Resource.Load<Sprite>("Icon_JoyCon.sprite");
    }

    void OnCllickJoystickNonFixed()
    {
        Managers.Sound.PlayButtonClick();
        Managers.Game.JoystickType = Define.EJoystickType.Flexible;
        GetButton((int)Buttons.JoystickFixedOnButton).gameObject.SetActive(false);
        GetButton((int)Buttons.JoystickFixedOffButton).gameObject.SetActive(true);
        GetImage((int)Images.JoyStickIconImage).sprite = Managers.Resource.Load<Sprite>("Icon_JoyCon_delete.sprite");
    }

    void OnClickBackgroundButton() // ´Ý±â ¹öÆ°
    {
        OnClose?.Invoke();
        gameObject.SetActive(false);
        Managers.Sound.Play(Define.ESound.Effect, "BackButton");
    }

}
