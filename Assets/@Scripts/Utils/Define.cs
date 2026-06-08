using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public static readonly float POINT_MIN_SIZE = 4f;
    public static readonly float POINT_MAX_SIZE = 7f;


    #region WAITFORSECOND
    public static readonly WaitForSeconds WAIT_2_SEC = new WaitForSeconds(2f);
    public static readonly WaitForSeconds WAIT_1_SEC = new WaitForSeconds(1f);
    public static readonly WaitForSeconds WAIT_HALF_SEC = new WaitForSeconds(0.5f);
    public static readonly WaitForSeconds WAIT_HALFOFHALF_SEC = new WaitForSeconds(0.25f);


    public static readonly WaitForSeconds WAIT_015_SEC = new WaitForSeconds(0.15f);
    public static readonly WaitForSeconds WAIT_02_SEC = new WaitForSeconds(0.2f);
    public static readonly WaitForSeconds WAIT_035_SEC = new WaitForSeconds(0.35f);



    #endregion
    public enum EObjectType
    {
        None,
        Player,
        Obstacle,
        Portal,
        Item,
        Ground,
        UI,
    }

    public enum EState
    {
        Roam,
        Chase,
        Escape
    }


    public enum EScene
    {
        None,
        TitleScene,
        LobbyScene,
        GameScene
    }
    public enum EJoystickType
    {
        Fixed,
        Flexible
    }
    public enum EUIEvent
    {
        Click,
        Pressed,
        PointerDown,
        PointerUp,
        BeginDrag,
        Drag,
        EndDrag,
    }

    public enum ESound
    {
        None,
        Bgm,
        SubBgm,
        Effect,
        Max,
    }

    public enum EItemType
    {
        None,
        Speed,
        Shield,
        Magnet,
        Point
    }


    public enum EColor
    {
        Green,
        Mint,
        Purple,
        Red,
        Yellow
    }

    public enum EShopType
    {
        Accessory,  
        Head,              
        Tail        
    }

}
