using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using static UnityEngine.Rendering.DebugUI;

public class SpeedController : ItemController
{

    void Awake()
    {
        _type = EItemType.Speed;
        duration = 10f;
    }

}
