// UI_PieceSlot.cs

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PieceSlot : UI_Base
{
    string _spriteName;
    ScrollRect _scrollRect;
    public Action OnItemClick;
    bool _isDrag = false;
    private bool _isAnimating = false;

    private static UI_PieceSlot _currentSelectedSlot;

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

        if (_currentSelectedSlot != null && _currentSelectedSlot != this)
        {
            _currentSelectedSlot.CloseEyes();
        }

        _currentSelectedSlot = this;

        OpenEyes();

        PlayScaleAnimation();
        EquipCharacter(_spriteName);
        OnItemClick?.Invoke();
    }

    //  public으로 변경
    public void OpenEyes()
    {
        GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(_spriteName);
    }

    //  public으로 변경
    public void CloseEyes()
    {
        GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(_spriteName + "_EyesClosed");
    }

    //  외부에서 선택 상태로 설정
    public void SetAsSelected()
    {
        _currentSelectedSlot = this;
    }

    void EquipCharacter(string spritename)
    {
        Managers.Game.PlayerSpriteNames[1] = spritename;
        Managers.Game.PlayerSpriteNames[2] = spritename;
        Managers.Game.PlayerSpriteNames[3] = spritename;
        Managers.Game.PlayerSpriteNames[4] = spritename;
    }

    public void SetInfo(string sprtiename, ScrollRect scrollRect)
    {
        _scrollRect = scrollRect;
        _spriteName = sprtiename;

        // 처음엔 눈 감은 상태로
        GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(sprtiename + "_EyesClosed");
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

    private void OnDisable()
    {
        if (_currentSelectedSlot == this)
        {
            _currentSelectedSlot = null;
        }
    }
}