using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UI_AccessoryPopup : UI_Popup
{
    public ScrollRect _scrollRect;
    public Action OnSaveButton;
    public Action<string, bool> OnAccessoryChange;
    AccessoryData _accessoryData;
    AccessoryData _preAccessoryData;

    UI_AccessoryItem _uI_AccessoryItem;

    //  팝업 열릴 때의 원래 착용 아이템 저장
    private int _originalEquippedIndex = -1;

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
    }

    enum Texts
    {
        BodyValueText,
        ItemName,
        ItemDescription,
        ItemCostText
    }
    enum Images
    {
    }
    #endregion

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        //  팝업 열릴 때 원래 착용 중이던 아이템 저장
        _originalEquippedIndex = Managers.Game.EquippedAccessoryIndex;

        Refresh();

        _accessoryData = null;
        Util.ShowUI(GetObject((int)GameObjects.ContentObject), new Vector2(-500, 0), 0.6f);
        Util.ShowUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, -500), 0.6f);

        GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(false);
    }

    //  팝업이 비활성화될 때 호출
    private void OnDisable()
    {
        // 현재 착용 중인 아이템이 구매하지 않은 아이템이면 원래 아이템으로 복구
        int currentEquipped = Managers.Game.EquippedAccessoryIndex;

        if (currentEquipped != -1 &&
            (!Managers.Game.AccessoryOwned.ContainsKey(currentEquipped) ||
             !Managers.Game.AccessoryOwned[currentEquipped]))
        {
            // 구매하지 않은 아이템이 착용됨 → 원래 아이템으로 복구
            Managers.Game.EquippedAccessoryIndex = _originalEquippedIndex;

            // UI 업데이트 (원래 아이템 스프라이트로)
            if (_originalEquippedIndex != -1 && Managers.Data.AccessoryDic.ContainsKey(_originalEquippedIndex))
            {
                AccessoryData originalData = Managers.Data.AccessoryDic[_originalEquippedIndex];
                OnAccessoryChange?.Invoke(originalData.SpriteName, true);
            }
            else
            {
                // 원래 아무것도 안 끼고 있었으면 null 또는 기본값
                OnAccessoryChange?.Invoke("", false);
            }
        }
    }

    private void Refresh(int purchasedIndex = -1)
    {
        GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(false);
        GetText((int)Texts.BodyValueText).text = Managers.Game.BodyCount.ToString();

        GameObject container = GetObject((int)GameObjects.AccessoryGroupObject);
        container.DestroyChilds();

        foreach (AccessoryData Data in Managers.Data.AccessoryDic.Values)
        {
            UI_AccessoryItem item = Managers.UI.MakeSubItem<UI_AccessoryItem>(container.transform);
            item.SetInfo(Data, _scrollRect);

            if (purchasedIndex == Data.CustomIndex)
            {
                item.FXPurchase();
            }

            item.OnItemClick = () =>
            {
                _uI_AccessoryItem = item;
                _accessoryData = Data;
                OnAccessoryChange?.Invoke(_accessoryData.SpriteName, Managers.Game.AccessoryOwned[Data.CustomIndex]);

                GetText((int)Texts.ItemName).text = Data.Name;
                GetText((int)Texts.ItemDescription).text = Data.Description;
                GetText((int)Texts.ItemCostText).text = Data.Cost.ToString();

                if (Managers.Game.AccessoryOwned[Data.CustomIndex])
                    GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(false);
                else
                {
                    GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(true);

                    if (Managers.Game.BodyCount < _accessoryData.Cost)
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

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.PurchaseButton).gameObject.BindEvent(OnClickPurchaseButton);

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

        if (_accessoryData == null)
        {
            OnSaveButton?.Invoke();
            Util.ShowUI(GetObject((int)GameObjects.ContentObject), new Vector2(500, 0), 0.6f);
            Util.ShowUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, 500), 0.6f);
            gameObject.SetActive(false);
            return;
        }

        if (!Managers.Game.AccessoryOwned[_accessoryData.CustomIndex])
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
        if (_accessoryData == null || Managers.Game.BodyCount < _accessoryData.Cost)
            return;

        int purchasedIndex = _accessoryData.CustomIndex;

        Managers.Game.BodyCount -= _accessoryData.Cost;
        Managers.Game.AccessoryOwned[purchasedIndex] = true;
        Managers.Game.SetAccessoryOwned(purchasedIndex, true);

        //  구매했으니 이제 원래 착용 아이템을 구매한 아이템으로 업데이트
        _originalEquippedIndex = purchasedIndex;

        Refresh(purchasedIndex);

        Managers.Sound.Play(Define.ESound.Effect, "Shop_Buy_01");
    }

    private void OnClickSaveButton()
    {
        if (_accessoryData == null)
        {
            OnSaveButton?.Invoke();
            Util.ShowUI(GetObject((int)GameObjects.ContentObject), new Vector2(500, 0), 0.6f);
            Util.ShowUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, 500), 0.6f);
            gameObject.SetActive(false);
            return;
        }

        if (!Managers.Game.AccessoryOwned[_accessoryData.CustomIndex])
        {
            Managers.UI.ShowToast("구매 후 저장하세요!");
            return;
        }

        OnSaveButton?.Invoke();
        Util.ShowUI(GetObject((int)GameObjects.ContentObject), new Vector2(500, 0), 0.6f);
        Util.ShowUI(GetObject((int)GameObjects.TopGroup), new Vector2(0, 500), 0.6f);

        gameObject.SetActive(false);
    }
}