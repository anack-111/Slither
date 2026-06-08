using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
    public GameObject[] activeObjects;


    private void Awake()
    {
        Init();

        SceneChangeAnimation_Out anim = Managers.Resource.Instantiate("SceneChangeAnimation_Out").GetOrAddComponent<SceneChangeAnimation_Out>();
        anim.SetInfo(SceneType, () => { });
    }
    protected override void Init()
    {


        base.Init();

        SceneType = Define.EScene.LobbyScene;
        
        Managers.UI.ShowSceneUI<UI_LobbyScene>();

        Managers.Sound.Play(Define.ESound.Bgm, "Bgm_Lobby");


    }

    public override void Clear()
    {
        if (activeObjects != null)
        {
            foreach (var obj in activeObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false); // 오브젝트 비활성화
                }
            }
        }
    }

}
