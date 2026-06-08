using Data;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_AccessoryItem : UI_Base
{

    AccessoryData _data;
    ScrollRect _scrollRect;
    public Action OnItemClick;
    bool _isDrag = false;
    private bool _isAnimating = false;

    #region Enum
    enum GameObjects
    {
        ContentObject,
        LockObject,
        Fx_Purchase
    }
    enum Buttons
    {
 
    }

    enum Texts
    {

    }
    enum Images
    {
        ItemImage
    }
    #endregion

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {

    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        gameObject.BindEvent(null, OnDrag, Define.EUIEvent.Drag);
        gameObject.BindEvent(null, OnBeginDrag, Define.EUIEvent.BeginDrag);
        gameObject.BindEvent(null, OnEndDrag, Define.EUIEvent.EndDrag);

        //GetObject((int)GameObjects.ContentObject).GetOrAddComponent<UI_ButtonAnimation>();

        gameObject.BindEvent(OnClickItem);

        GetObject((int)GameObjects.Fx_Purchase).SetActive(false);   


        return true;
    }

    private void OnClickItem()
    {
        PlayScaleAnimation();
        Managers.Sound.PlayButtonClick();

        if (_isDrag) return;

        OnItemClick?.Invoke();
        EquipAccessory(_data.CustomIndex);

    }

    void EquipAccessory(int customIndex)
    {
        if (Managers.Game.AccessoryOwned.ContainsKey(customIndex) &&
            Managers.Game.AccessoryOwned[customIndex] == true)
        {
            Managers.Game.EquippedAccessoryIndex = customIndex;
        }
        else
        {
            Managers.Game.EquippedAccessoryIndex = customIndex;
            Debug.Log("아이템을 소유하지 않았습니다.");
        }
    }

    public void SetInfo(AccessoryData data, ScrollRect scrollRect)
    {
        _scrollRect = scrollRect;
        _data = data;

        GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(data.SpriteName);


        bool owned = Managers.Game.AccessoryOwned.ContainsKey(data.CustomIndex) &&
                     Managers.Game.AccessoryOwned[data.CustomIndex];

        GetObject((int)GameObjects.LockObject).SetActive(!owned);
        GetObject((int)GameObjects.Fx_Purchase).SetActive(false);
    }

    public void FXPurchase()
    {
        GameObject fx = GetObject((int)GameObjects.Fx_Purchase);
        fx.SetActive(false);  // 일단 끄고
        fx.SetActive(true);   // 다시 켜서 파티클 재시작
    }

    public int GetCustomIndex()
    {
        return _data != null ? _data.CustomIndex : -1;
    }

    public void OnDrag(BaseEventData baseEventData)
    {
        _isDrag = true;
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnDrag(pointerEventData);
    }

    public void OnBeginDrag(BaseEventData baseEventData)
    {
        _isDrag = true;
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnBeginDrag(pointerEventData);
    }

    public void OnEndDrag(BaseEventData baseEventData)
    {
        _isDrag = false;
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        _scrollRect.OnEndDrag(pointerEventData);
    }
    void PlayScaleAnimation()
    {
        _isAnimating = true;
        Transform imageTransform = GetImage((int)Images.ItemImage).transform;

        imageTransform.DOKill();

        Vector3 originalScale = Vector3.one;
        Vector3 largeScale = Vector3.one * 1.3f;

        imageTransform.localScale = originalScale;

        imageTransform.DOScale(largeScale, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                imageTransform.DOScale(originalScale, 0.1f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        _isAnimating = false;
                    });
            });
    }
}
