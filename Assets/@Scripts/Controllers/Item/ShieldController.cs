using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ShieldController : ItemController
{

    void Awake()
    {
        _type = EItemType.Shield;
        duration = 5f;
    }
}
