// UI_ShopPopup.cs (UI_AccessoryPopup 이름 변경)

using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UI_ShopPopup : UI_Popup
{
    public ScrollRect _scrollRect;
    public Action OnSaveButton;
    public Action<string> OnItemChange;

    //  현재 팝업 타입
    private Define.EShopType _shopType;

    private AccessoryData _selectedData;
    private UI_AccessoryItem _selectedItem;

    #region Enum
    enum GameObjects
    {
        ContentObject,
        TopGroup,
        AccessoryGroupObject,
        BoardObject,
        BodyObject
    }

    enum Buttons
    {
        PurchaseButton,
        PrevButton
    }

    enum Texts
    {
        BodyValueText,
        ItemName,
        ItemDescription,
        ItemCostText,
        TitleText  //  타이틀 추가
    }

    enum Images { }
    #endregion

    private void Awake()
    {
        Init();
    }

    //  팝업 열 때 타입 설정
    public void SetShopType(Define.EShopType shopType)
    {
        _shopType = shopType;

        // 타이틀 변경
        string title = shopType switch
        {
            Define.EShopType.Accessory => "악세사리 상점",
            Define.EShopType.Head => "머리 스타일",
           // Define.EShopType.Body => "몸통 스타일",
            Define.EShopType.Tail => "꼬리 스타일",
            _ => "상점"
        };

        GetText((int)Texts.TitleText).text = title;
    }

    private void OnEnable()
    {
        Refresh();

        _selectedData = null;
        Util.ShowUI(GetObject((int)GameObjects.ContentObject), new Vector2(-500, 0), 0.6f);
        Util.ShowUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, -500), 0.6f);

        GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(false);
    }

    private void Refresh(int purchasedIndex = -1)
    {
        GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(false);
        GetText((int)Texts.BodyValueText).text = Managers.Game.BodyCount.ToString();

        GameObject container = GetObject((int)GameObjects.AccessoryGroupObject);
        container.DestroyChilds();

        //  타입에 따라 다른 데이터 로드
        Dictionary<int, AccessoryData> dataDict = GetDataDictionary();

        foreach (AccessoryData Data in dataDict.Values)
        {
            UI_AccessoryItem item = Managers.UI.MakeSubItem<UI_AccessoryItem>(container.transform);
            item.SetInfo(Data, _scrollRect);

            if (purchasedIndex == Data.CustomIndex)
            {
                item.FXPurchase();
            }

            item.OnItemClick = () =>
            {
                _selectedItem = item;
                _selectedData = Data;
                OnItemChange?.Invoke(_selectedData.SpriteName);

                GetText((int)Texts.ItemName).text = Data.Name;
                GetText((int)Texts.ItemDescription).text = Data.Description;
                GetText((int)Texts.ItemCostText).text = Data.Cost.ToString();

                //  타입에 맞는 소유 여부 확인
                if (IsOwned(Data.CustomIndex))
                    GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(false);
                else
                {
                    GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(true);

                    if (Managers.Game.BodyCount < _selectedData.Cost)
                    {
                        GetObject((int)GameObjects.BoardObject).SetActive(true);
                        GetButton((int)Buttons.PurchaseButton).interactable = false;
                    }
                    else
                    {
                        GetObject((int)GameObjects.BoardObject).SetActive(false);
                        GetButton((int)Buttons.PurchaseButton).interactable = true;
                    }
                }
            };
        }
    }

    //  타입에 따라 데이터 딕셔너리 반환
    Dictionary<int, AccessoryData> GetDataDictionary()
    {
        return _shopType switch
        {
            Define.EShopType.Accessory => Managers.Data.AccessoryDic,
            //Define.EShopType.Head => Managers.Data.HeadDic,
            //Define.EShopType.Body => Managers.Data.BodyDic,
            //Define.EShopType.Tail => Managers.Data.TailDic,
            _ => Managers.Data.AccessoryDic
        };
    }

    //  타입에 따라 소유 여부 확인
    bool IsOwned(int index)
    {
        return _shopType switch
        {
            Define.EShopType.Accessory => Managers.Game.AccessoryOwned[index],
            //Define.EShopType.Head => Managers.Game.HeadOwned[index],
            //Define.EShopType.Tail => Managers.Game.TailOwned[index],
            _ => false
        };
    }

    //  타입에 따라 소유 설정
    void SetOwned(int index, bool value)
    {
        switch (_shopType)
        {
            case Define.EShopType.Accessory:
                Managers.Game.AccessoryOwned[index] = value;
                Managers.Game.SetAccessoryOwned(index, value);
                break;
            //case Define.EShopType.Head:
            //    Managers.Game.HeadOwned[index] = value;
            //    Managers.Game.SetHeadOwned(index, value);
            //    break;
            //case Define.EShopType.Body:
            //    Managers.Game.BodyOwned[index] = value;
            //    Managers.Game.SetBodyOwned(index, value);
            //    break;
            //case Define.EShopType.Tail:
            //    Managers.Game.TailOwned[index] = value;
            //    Managers.Game.SetTailOwned(index, value);
            //    break;
        }
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.PurchaseButton).gameObject.BindEvent(OnClickPurchaseButton);
        GetButton((int)Buttons.PrevButton).gameObject.BindEvent(OnClickPrevButton);
        GetButton((int)Buttons.PrevButton).gameObject.GetOrAddComponent<UI_ButtonAnimation>();

        GetObject((int)GameObjects.BodyObject).gameObject.GetOrAddComponent<UI_ButtonAnimation>();
        GetObject((int)GameObjects.BodyObject).BindEvent(OnClickStoreButton);

        return true;
    }

    private void OnClickStoreButton()
    {
        Managers.UI.ShowToast("Coming Soon!");
        Managers.Sound.Play(Define.ESound.Effect, "Button_Toast");
    }

    private void OnClickPrevButton()
    {
        Managers.Sound.Play(Define.ESound.Effect, "BackButton");

        if (_selectedData == null)
        {
            OnSaveButton?.Invoke();
            Util.ShowUI(GetObject((int)GameObjects.ContentObject), new Vector2(500, 0), 0.6f);
            Util.ShowUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, 500), 0.6f);
            gameObject.SetActive(false);
            return;
        }

        if (!IsOwned(_selectedData.CustomIndex))
        {
            Managers.UI.ShowToast("구매 후 저장하세요!");
            Managers.Sound.Play(Define.ESound.Effect, "Button_Toast");
            return;
        }

        OnSaveButton?.Invoke();
        Util.ShowUI(GetObject((int)GameObjects.ContentObject), new Vector2(500, 0), 0.6f);
        Util.ShowUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, 500), 0.6f);
        gameObject.SetActive(false);
    }

    private void OnClickPurchaseButton()
    {
        if (_selectedData == null || Managers.Game.BodyCount < _selectedData.Cost)
            return;

        int purchasedIndex = _selectedData.CustomIndex;

        Managers.Game.BodyCount -= _selectedData.Cost;
        SetOwned(purchasedIndex, true);

        Refresh(purchasedIndex);

        Managers.Sound.Play(Define.ESound.Effect, "Shop_Buy_01");
    }
}