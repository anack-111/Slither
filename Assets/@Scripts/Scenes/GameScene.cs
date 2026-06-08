using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class GameScene : BaseScene
{
     
    public CameraController cameraController;

    public UI_Rank _ui_rank;
    private void Awake()
    {
        Init();

        QualitySettings.vSyncCount = 0;
        SceneChangeAnimation_Out anim = Managers.Resource.Instantiate("SceneChangeAnimation_Out").GetOrAddComponent<SceneChangeAnimation_Out>();
        anim.SetInfo(SceneType, () => { });


    }

    private void Start()
    {
        SetupPhysicsLayers(); 

        QualitySettings.SetQualityLevel(0);  // Fastest
        QualitySettings.pixelLightCount = 0;
        QualitySettings.shadowDistance = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        Time.fixedDeltaTime = 0.03f;  //  0.02 → 0.03 (물리 연산 33% 감소)

        Physics.defaultSolverIterations = 3;  //  6 → 3
        Physics.defaultSolverVelocityIterations = 1;
    }

    protected override void Init()
    {


        base.Init();
        PlayerController player = Managers.Object.Spawn<PlayerController>(new Vector3(0, 0, 0), "Player");
        player.Init();


        SceneType = Define.EScene.GameScene;
        _ui_rank = Managers.UI.ShowSceneUI<UI_Rank>();
        Managers.UI.ShowSceneUI<UI_GameScene>();

        Managers.Game.Kill = 0;
        Managers.Sound.Play(Define.ESound.Bgm, "Bgm_Game");
       
    }

    public float playTime = 0f;

    void Update()
    {
        if (!Managers.Object.Player.parent.isDead)
        {
            playTime += Time.deltaTime;
        }
    }

    void SetupPhysicsLayers()
    {
        //  불필요한 레이어 간 충돌 끄기
        // Edit > Project Settings > Physics > Layer Collision Matrix

        int pointLayer = LayerMask.NameToLayer("Points");
        int creatureLayer = LayerMask.NameToLayer("Default");
        int itemLayer = LayerMask.NameToLayer("Items");

        // Point끼리는 충돌 안 함
        Physics.IgnoreLayerCollision(pointLayer, pointLayer, true);

        // Item끼리는 충돌 안 함
        Physics.IgnoreLayerCollision(itemLayer, itemLayer, true);
    }
    public override void Clear()
    {
        Managers.Game.MaxPlayTime = playTime;

    }
}
