using CnControls;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;



public class UI_Skill : UI_Scene
{

    
    public class ItemData
    {
        public EItemType type;
        public float buffMultiplier;
        public float duration;

    }
 
    public Animator sprintAnim;

    ItemData _currentItem;

    // 조이스틱 기본 위치 저장
    enum Buttons
    {
        SprintButton,
        ItemButton

    }

    enum Images
    {
        ItemImage,
        SprintBackborad
    }


    private VirtualButton _jumpButton;

    public override bool Init()
    {
        if (!base.Init())
            return false;


        BindButton(typeof(Buttons));
        BindImage(typeof(Images));

         _jumpButton = new VirtualButton("Jump");


         CnInputManager.RegisterVirtualButton(_jumpButton);

        GetButton((int)Buttons.SprintButton).gameObject.BindEvent(OnSprintButtonDown, null, Define.EUIEvent.PointerDown);
        GetButton((int)Buttons.SprintButton).gameObject.BindEvent(OnSprintButtonUp, null, Define.EUIEvent.PointerUp);

        GetButton((int)Buttons.ItemButton).gameObject.BindEvent(UseItem);

        GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>("Default.sprite");

        GetImage((int)Images.SprintBackborad).gameObject.SetActive(false);

        sprintAnim.Play("Anim_Release", -1, 0f);

        return true;
    }

    // =========================
    //   고정형 조이스틱
    // =========================

    public void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            OnSprintButtonDown();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            OnSprintButtonUp();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            UseItem();
        }


    }

    public void OnSprintButtonDown()
    {
        _jumpButton.Press();
        sprintAnim.Play("Anim_Press", -1, 0f);
        GetImage((int)Images.SprintBackborad).gameObject.SetActive(true);
    }

    public void OnSprintButtonUp()
    {
        _jumpButton.Release();
        sprintAnim.Play("Anim_Release", -1, 0f);
        GetImage((int)Images.SprintBackborad).gameObject.SetActive(false);
    }

    public void ForceSprintOff()
    {
        _jumpButton.Release();
        GetImage((int)Images.SprintBackborad).gameObject.SetActive(false);
    }

    public void ItemSet(ItemController itemController)
    {
        // 아이템 정보만 복사
        _currentItem = new ItemData
        {
            type = itemController._type,
            buffMultiplier = itemController.buffMultiplier,
            duration = itemController.duration
        };

        // UI 아이콘 세팅
        GetImage((int)Images.ItemImage).sprite =
            Managers.Resource.Load<Sprite>(_currentItem.type.ToString() + ".sprite");
    }
    public void UseItem()
    {
        if (_currentItem == null)
            return;

        Managers.Object.Player.parent.ApplyItemBuff(
            _currentItem.type,
            _currentItem.buffMultiplier,
            _currentItem.duration
        );

        _currentItem = null;
        GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>("Default.sprite");
    }

    public void OnDestroy()
    {
        CnInputManager.UnregisterVirtualButton(_jumpButton);
    }
}
