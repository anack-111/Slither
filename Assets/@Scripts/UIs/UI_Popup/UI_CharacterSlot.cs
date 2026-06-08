using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class UI_CharacterSlot : UI_Base
{
    string _spriteName;
    ScrollRect _scrollRect;
    public Action OnItemClick;
    bool _isDrag = false;

    //  애니메이션 중복 방지
    private bool _isAnimating = false;

    #region Enum
    enum GameObjects
    {
        ContentObject,
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
        gameObject.BindEvent(OnClickItem);

        return true;
    }

    private void OnClickItem()
    {
        Managers.Sound.PlayButtonClick();

        if (_isDrag) return;

        //  애니메이션 중이면 리턴
        if (_isAnimating) return;

        EquipCharacter(_spriteName);
        OnItemClick?.Invoke();

        // 스케일 애니메이션
        PlayScaleAnimation();
    }

    void PlayScaleAnimation()
    {
        _isAnimating = true;

        Transform imageTransform = GetImage((int)Images.ItemImage).transform;

        //  기존 애니메이션 정리
        imageTransform.DOKill();

        Vector3 originalScale = Vector3.one;
        Vector3 largeScale = Vector3.one * 1.3f;

        //  커졌다가 작아지기
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

    void EquipCharacter(string spritename)
    {
        Managers.Game.PlayerSpriteNames[0] = spritename;
    }

    public void SetInfo(string sprtiename, ScrollRect scrollRect)
    {
        _scrollRect = scrollRect;
        _spriteName = sprtiename;
        string name = sprtiename.Replace("_Head", "");
        GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(name);
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
}