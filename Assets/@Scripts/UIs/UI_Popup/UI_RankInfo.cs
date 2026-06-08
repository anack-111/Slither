using Data;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_RankInfo : UI_Base
{
    ScrollRect _scrollRect;
    bool _isDrag = false;

    //  애니메이션 중복 방지
    private bool _isAnimating = false;

    #region Enum
    enum GameObjects
    {
        Rank3
    }
    enum Buttons
    {
    }
    enum Texts
    {
        RankTextValue,
        KillTextValue,
        PlayerNameText
    }
    enum Images
    {
        PlayerImage,
        PlayerAC,

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
       // Managers.Sound.PlayButtonClick();

        if (_isDrag) return;
    }

    public void SetInfo(RankData data, ScrollRect scrollRect, string spriteName, string accessoryName, int rank)
    {
        //  Init이 안 되어 있으면 먼저 호출
        if (!_init)
            Init();

    
        _scrollRect = scrollRect;

        GetImage((int)Images.PlayerAC).sprite = Managers.Resource.Load<Sprite>(accessoryName);

        if (rank == 3)
        {
            GetObject((int)GameObjects.Rank3).SetActive(true);
            GetText((int)Texts.RankTextValue).gameObject.SetActive(false);
        }
        else
        {
            GetObject((int)GameObjects.Rank3).SetActive(false);
            GetText((int)Texts.RankTextValue).gameObject.SetActive(true);
        }

            string name = spriteName.Replace("_Head", "");
        GetImage((int)Images.PlayerImage).sprite = Managers.Resource.Load<Sprite>(name);
        GetText((int)Texts.RankTextValue).text = rank.ToString();
        GetText((int)Texts.KillTextValue).text = data.KillPoint.ToString();

        if (data.name == null)
            GetText((int)Texts.PlayerNameText).text = "플레이어";
        else
            GetText((int)Texts.PlayerNameText).text = data.name.ToString();
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
