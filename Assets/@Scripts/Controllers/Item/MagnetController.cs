using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetController : ItemController
{

    public int MagnetValue = 50;
    protected override void OnTriggerEnter(Collider collision)
    {
        if(collision.CompareTag("Creature"))
        {

            var creature = collision.GetComponentInParent<Creature>();
            if (creature == null)
                return;


            if (!collision.GetComponent<PlayerController>().IsPlayer)
            {

                creature.ApplyItemBuff(_type, buffMultiplier, duration);
            }
            else
            {
                UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
                ui._skillUI.ItemSet(this);
                Managers.Sound.Play(Define.ESound.Effect, "ItemSound");
            }

            Destroy(gameObject);

        }
    }
}
