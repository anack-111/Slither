using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerName : UI_Base
{
    #region Enum
    enum GameObjects
    {

    }
    enum Buttons { }
    enum Texts
    {
        PlayerNameText

    }
    #endregion

    Camera _cam;
    PlayerController _playerController;
    public Vector3 offset = new Vector3(0, 2, 0);
    private void Awake()
    {
        Init();
        _cam = Camera.main;
    }


    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));


        return true;
    }

    public void SetInfo(PlayerController controller)
    {
        _playerController = controller; 
        GetText((int)Texts.PlayerNameText).text = controller.parent._name; 
    }

    private void LateUpdate()
    {

        if( _playerController != null)
        {
            Vector3 targetPosition = _playerController.transform.position + offset;

            // UI 이름의 월드 공간 위치를 목표 위치로 업데이트
            transform.position = targetPosition;

            // UI 이름이 카메라를 향하도록 설정 (회전은 하지 않음)
            transform.rotation = Quaternion.LookRotation(_cam.transform.forward, Vector3.up);
        }
    
    
    }

}
