using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ItemController : BaseController
{
    public EItemType _type;
    public float buffMultiplier = 1.3f;
    public float duration = 10f;

    protected virtual void OnTriggerEnter(Collider obj)
    {

        var creature = obj.GetComponentInParent<Creature>();
        if (creature == null)
            return;

        if (obj.transform.tag == "Creature")
        {
          
 

            if(creature._head.IsPlayer)
            {
                UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
                ui._skillUI.ItemSet(this);
                Managers.Sound.Play(Define.ESound.Effect, "ItemSound");

            }
            else
            {
                creature.ApplyItemBuff(_type, buffMultiplier, duration);
            }

            Destroy(gameObject);
        }
    }
}
