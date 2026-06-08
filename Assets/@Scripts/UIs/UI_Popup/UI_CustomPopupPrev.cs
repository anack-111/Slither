using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UI_CustomPopupPrev : UI_Popup
{
    CustomData _customData;

    public VerticalScrollSnap _1scrollsnap;
    public VerticalScrollSnap _2scrollsnap;
    public VerticalScrollSnap _3scrollsnap;
    public VerticalScrollSnap _4scrollsnap;
    public VerticalScrollSnap _5scrollsnap;

    UI_AccessoryPopup _AccessoryPopup;

    public Action OnCloseButton;

    private bool isFirstTime = true;

    #region Enum
    enum GameObjects
    {
        ContentObject,
        HeadScrollContentObject,
        FirstScrollContentObject,
        SecondScrollContentObject,
        ThirdScrollContentObject,
        ForthScrollContentObject,
        TopGroupObject,
        BottomGroupObject,
        ArrowObject1,
        UpArrowImage1,
        UpArrowImage2,
        UpArrowImage3,
        UpArrowImage4,
        UpArrowImage5,
        DownArrowImage1,
        DownArrowImage2,
        DownArrowImage3,
        DownArrowImage4,
        DownArrowImage5,
        //  클릭 영역용 오브젝트 추가
        UpArrowImageObject1,
        UpArrowImageObject2,
        UpArrowImageObject3,
        UpArrowImageObject4,
        UpArrowImageObject5,
        DownArrowImageObject1,
        DownArrowImageObject2,
        DownArrowImageObject3,
        DownArrowImageObject4,
        DownArrowImageObject5,
    }

    enum Buttons
    {
        AccessoryButton,
        CloseButton,
        SaveButton
    }

    enum Images
    {
        HeadBGImage
    }
    #endregion

    // 임시 저장용 (저장 안 하면 폐기됨)
    private Dictionary<int, int> tempIndexes = new Dictionary<int, int>();
    private Dictionary<int, string> scrollSpriteNames = new Dictionary<int, string>();
    private bool isMutedForFirstFewSeconds = true;


    private void Awake()
    {
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _AccessoryPopup = Managers.UI.ShowPopupUI<UI_AccessoryPopup>();
        //_AccessoryPopup.OnAccessoryChange += RefreshAccessory;


        _AccessoryPopup.gameObject.SetActive(false);


        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.CloseButton).GetOrAddComponent<UI_ButtonAnimation>();

        GetButton((int)Buttons.SaveButton).gameObject.BindEvent(OnClickSaveButton);
        GetButton((int)Buttons.SaveButton).GetOrAddComponent<UI_ButtonAnimation>();


        GetButton((int)Buttons.AccessoryButton).gameObject.BindEvent(OnClickAccessoryButton);
        GetButton((int)Buttons.AccessoryButton).GetOrAddComponent<UI_ButtonAnimation>();



        _1scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) => OnChangeScroll(idx, 1));
        _2scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) => OnChangeScroll(idx, 2));
        _3scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) => OnChangeScroll(idx, 3));
        _4scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) => OnChangeScroll(idx, 4));
        _5scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) => OnChangeScroll(idx, 5));




        //  클릭 영역 오브젝트에 애니메이션 추가
        GetObject((int)GameObjects.UpArrowImageObject1).GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.UpArrowImageObject2).GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.UpArrowImageObject3).GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.UpArrowImageObject4).GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.UpArrowImageObject5).GetOrAddComponent<UI_ButtonAnimation>();

        GetObject((int)GameObjects.DownArrowImageObject1).GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.DownArrowImageObject2).GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.DownArrowImageObject3).GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.DownArrowImageObject4).GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.DownArrowImageObject5).GetOrAddComponent<UI_ButtonAnimation>();


        #region ARROW BindEvent
        //  클릭 영역 오브젝트에 이벤트 바인딩
        GetObject((int)GameObjects.UpArrowImageObject1).BindEvent(() => _1scrollsnap.NextScreen());
        GetObject((int)GameObjects.DownArrowImageObject1).BindEvent(() => _1scrollsnap.PreviousScreen());
        GetObject((int)GameObjects.UpArrowImageObject2).BindEvent(() => _2scrollsnap.NextScreen());
        GetObject((int)GameObjects.DownArrowImageObject2).BindEvent(() => _2scrollsnap.PreviousScreen());
        GetObject((int)GameObjects.UpArrowImageObject3).BindEvent(() => _3scrollsnap.NextScreen());
        GetObject((int)GameObjects.DownArrowImageObject3).BindEvent(() => _3scrollsnap.PreviousScreen());
        GetObject((int)GameObjects.UpArrowImageObject4).BindEvent(() => _4scrollsnap.NextScreen());
        GetObject((int)GameObjects.DownArrowImageObject4).BindEvent(() => _4scrollsnap.PreviousScreen());
        GetObject((int)GameObjects.UpArrowImageObject5).BindEvent(() => _5scrollsnap.NextScreen());
        GetObject((int)GameObjects.DownArrowImageObject5).BindEvent(() => _5scrollsnap.PreviousScreen());

        #endregion

        // RestartOnEnable = false → 스크롤 튀는 원흉 제거
        _1scrollsnap.RestartOnEnable = false;
        _2scrollsnap.RestartOnEnable = false;
        _3scrollsnap.RestartOnEnable = false;
        _4scrollsnap.RestartOnEnable = false;
        _5scrollsnap.RestartOnEnable = false;

        Refresh();
        return true;
    }


    private bool areEventListenersAdded = false;
    private void OnEnable()
    {
        StartCoroutine(Co_SetSavedPages());
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
        StartCoroutine(EnableSoundAfterDelay(0.3f));

        if (!areEventListenersAdded)
        {
            AddScrollListenersWithSound();
            areEventListenersAdded = true;
        }
    }

    private void OnDisable()
    {
        // 팝업이 비활성화될 때, 소리 초기화
        isMutedForFirstFewSeconds = true; // 팝업을 다시 활성화할 때, 소리 비활성화 상태로 초기화
    }

    private IEnumerator EnableSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isMutedForFirstFewSeconds = false; // 0.3초 뒤에 소리 활성화
    }

    private void AddScrollListenersWithSound()
    {
        // 스크롤 페이지 변경 시 소리 추가
        _1scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) =>
        {
            OnChangeScroll(idx, 1);
            PlayScrollSound();
        });
        _2scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) =>
        {
            OnChangeScroll(idx, 2);
            PlayScrollSound();
        });
        _3scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) =>
        {
            OnChangeScroll(idx, 3);
            PlayScrollSound();
        });
        _4scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) =>
        {
            OnChangeScroll(idx, 4);
            PlayScrollSound();
        });
        _5scrollsnap.OnSelectionPageChangedEvent.AddListener((idx) =>
        {
            OnChangeScroll(idx, 5);
            PlayScrollSound();
        });
    }
    private void PlayScrollSound()
    {
        if (isMutedForFirstFewSeconds) return; // 처음 0.3초 동안 소리 안나게

        Managers.Sound.Play(Define.ESound.Effect, "ScrollButton");
    }

    private void OnDestroy()
    {
        //_AccessoryPopup.OnAccessoryChange -= RefreshAccessory;
    }
    private void OnClickAccessoryButton()
    {

        _AccessoryPopup.OnSaveButton = () =>
        {
            Util.ShowAndHideUI(GetObject((int)GameObjects.TopGroupObject), new Vector2(0, -500), 0.3f);
            Util.ShowUI(GetObject((int)GameObjects.BottomGroupObject), new Vector2(0, 500), 0.3f);
            Util.ShowCanvasGroup(GetObject((int)GameObjects.ArrowObject1), 0.3f);
            GetImage((int)Images.HeadBGImage).sprite = Managers.Resource.Load<Sprite>("PlayerBGOff.sprite");
        };

        Util.ShowAndHideUI(GetObject((int)GameObjects.TopGroupObject), new Vector2(0, 500), 0.3f);
        Util.ShowUI(GetObject((int)GameObjects.BottomGroupObject), new Vector2(0, -500), 0.3f);
        Util.HideCanvasGroup(GetObject((int)GameObjects.ArrowObject1), 0.3f);
        GetImage((int)Images.HeadBGImage).sprite = Managers.Resource.Load<Sprite>("PlayerBGOn.sprite");
        _AccessoryPopup.gameObject.SetActive(true);

        Managers.Sound.PlayButtonClick();
    }

    private void OnClickSaveButton()
    {
        for (int i = 1; i <= 5; i++)
        {
            Managers.Game.PlayerSpriteIdx[i - 1] = tempIndexes.ContainsKey(i) ? tempIndexes[i] : 0;
            Managers.Game.PlayerSpriteNames[i - 1] = scrollSpriteNames.ContainsKey(i) ? scrollSpriteNames[i] : "";
        }

        Managers.UI.ShowToast("저장 완료");
        Managers.Sound.Play(Define.ESound.Effect, "Button_Toast");
        Managers.Game.SaveGame();

    }

    private void OnClickCloseButton()
    {
        isMutedForFirstFewSeconds = false;
        OnCloseButton?.Invoke();
        gameObject.SetActive(false);

        Managers.Sound.Play(Define.ESound.Effect, "BackButton");

    }


    private IEnumerator Co_SetSavedPages()
    {
        yield return null; // ChildObjects 생성 완료 대기

        int[] saved = Managers.Game.PlayerSpriteIdx;

        _1scrollsnap.GoToScreen(saved[0]);
        _2scrollsnap.GoToScreen(saved[1]);
        _3scrollsnap.GoToScreen(saved[2]);
        _4scrollsnap.GoToScreen(saved[3]);
        _5scrollsnap.GoToScreen(saved[4]);

        // tempIndexes도 저장값으로 초기화
        for (int i = 1; i <= 5; i++)
            tempIndexes[i] = saved[i - 1];
    }


    private void Refresh()
    {
        if (!_init)
            return;

        scrollSpriteNames.Clear();

        if (Managers.Game.PlayerSpriteNames.Any(x => string.IsNullOrEmpty(x)))
        {
            for (int i = 1; i <= 5; i++)
            {
                string first = GetFirstSpriteName(i);
                scrollSpriteNames[i] = first;
                Managers.Game.PlayerSpriteNames[i - 1] = first;
                Managers.Game.PlayerSpriteIdx[i - 1] = 0;
            }
        }
        else
        {
            for (int i = 1; i <= 5; i++)
                scrollSpriteNames[i] = Managers.Game.PlayerSpriteNames[i - 1];
        }

        // HEAD
        GameObject headContainer = GetObject((int)GameObjects.HeadScrollContentObject);
        headContainer.DestroyChilds();

        var headList = Managers.Data.CustomDic.Values
            .OrderBy(cd => cd.CustomIndex)
            .ToList();

        _1scrollsnap.ChildObjects = new GameObject[headList.Count];

        foreach (var data in headList)
        {
            UI_CustomItem item = Managers.UI.MakeSubItem<UI_CustomItem>(headContainer.transform);
            item.SetInfo(data.SpriteName, true);
            _1scrollsnap.ChildObjects[data.CustomIndex - 1] = item.gameObject;
        }

        // CHILD 4개
        VerticalScrollSnap[] scrolls = { _2scrollsnap, _3scrollsnap, _4scrollsnap, _5scrollsnap };
        var childList = Managers.Data.CustomChildDic.Values
            .OrderBy(cd => cd.CustomIndex)
            .ToList();

        foreach (var scroll in scrolls)
            scroll.ChildObjects = new GameObject[childList.Count];

        foreach (var data in childList)
        {
            int idx = data.CustomIndex - 1;

            for (int i = 0; i < scrolls.Length; i++)
            {
                GameObject container = GetObject((int)GameObjects.FirstScrollContentObject + i);

                UI_CustomItem item = Managers.UI.MakeSubItem<UI_CustomItem>(container.transform);
                item.SetInfo(data.SpriteName);

                scrolls[i].ChildObjects[idx] = item.gameObject;
            }
        }
    }

    private string GetFirstSpriteName(int scrollNumber)
    {
        if (scrollNumber == 1)
            return Managers.Data.CustomDic.Values.OrderBy(cd => cd.CustomIndex).First().SpriteName;

        return Managers.Data.CustomChildDic.Values.OrderBy(cd => cd.CustomIndex).First().SpriteName;
    }


    void RefreshAccessory(string name)
    {
        foreach (var obj in _1scrollsnap.ChildObjects)
        {
            obj.GetComponent<UI_CustomItem>().SetAccessory(name);
        }
    }


    private void OnChangeScroll(int index, int scrollNumber)
    {
        tempIndexes[scrollNumber] = index;

        if (scrollNumber == 1)
        {
            var data = Managers.Data.CustomDic.Values
                .First(cd => cd.CustomIndex == index + 1);
            scrollSpriteNames[scrollNumber] = data.SpriteName;
        }
        else
        {
            var data = Managers.Data.CustomChildDic.Values
                .First(cd => cd.CustomIndex == index + 1);
            scrollSpriteNames[scrollNumber] = data.SpriteName;
        }

        // 첫 번째 페이지 또는 마지막 페이지에 따라 화살표 활성화/비활성화
        UpdateArrowButtonState(scrollNumber, index);
    }

    private void UpdateArrowButtonState(int scrollNumber, int index)
    {
        bool isFirstPage = false;
        bool isLastPage = false;

        //  클릭 영역 오브젝트로 변경
        GameObject upArrowObject = null;
        GameObject downArrowObject = null;
        GameObject[] childObjects = null;

        // 각 스크롤에 해당하는 화살표와 childObjects 설정
        switch (scrollNumber)
        {
            case 1:
                upArrowObject = GetObject((int)GameObjects.UpArrowImageObject1);
                downArrowObject = GetObject((int)GameObjects.DownArrowImageObject1);
                childObjects = _1scrollsnap.ChildObjects;
                break;
            case 2:
                upArrowObject = GetObject((int)GameObjects.UpArrowImageObject2);
                downArrowObject = GetObject((int)GameObjects.DownArrowImageObject2);
                childObjects = _2scrollsnap.ChildObjects;
                break;
            case 3:
                upArrowObject = GetObject((int)GameObjects.UpArrowImageObject3);
                downArrowObject = GetObject((int)GameObjects.DownArrowImageObject3);
                childObjects = _3scrollsnap.ChildObjects;
                break;
            case 4:
                upArrowObject = GetObject((int)GameObjects.UpArrowImageObject4);
                downArrowObject = GetObject((int)GameObjects.DownArrowImageObject4);
                childObjects = _4scrollsnap.ChildObjects;
                break;
            case 5:
                upArrowObject = GetObject((int)GameObjects.UpArrowImageObject5);
                downArrowObject = GetObject((int)GameObjects.DownArrowImageObject5);
                childObjects = _5scrollsnap.ChildObjects;
                break;
        }

        if (childObjects != null)
        {
            // 총 페이지 수 계산
            int totalPages = childObjects.Length;

            // 첫 번째 페이지와 마지막 페이지 체크
            isFirstPage = index == 0;          // 첫 번째 페이지 (index == 0)
            isLastPage = index == totalPages - 1; // 마지막 페이지 (index == totalPages - 1)

            // 첫 번째 페이지일 때 아래쪽 화살표 비활성화, 마지막 페이지일 때 위쪽 화살표 비활성화
            if (downArrowObject != null) downArrowObject.SetActive(!isFirstPage);  // 첫 번째 페이지일 때 아래 화살표 비활성화
            if (upArrowObject != null) upArrowObject.SetActive(!isLastPage); // 마지막 페이지일 때 위 화살표 비활성화
        }
    }
}
