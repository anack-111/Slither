using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_RankPopup : UI_Popup
{
    ScrollRect _scrollrect;

    #region Enum
    enum GameObjects
    {
        RankGroup,
        RankInfoScrollContentObject,
        BackgroundButton,
        ContentObject
    }
    enum Buttons
    {
    }
    enum Texts
    {
        Rank1KillTextValue,
        Rank2KillTextValue,
        Rank1NameTextValue,
        Rank2NameTextValue,
        PlayerRankTextValue,
        PlayerKillPointValue,
        PlayerText
    }
    enum Images
    {
        Rank1PlayerImage,
        Rank2PlayerImage,
        PlayerImage,
        PlayerAC,
        Rank1PlayerAC,
        Rank2PlayerAC
    }
    #endregion

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Refresh();
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }

    private void Refresh()
    {
        string playerName = Managers.Game.PlayerName;
        int playerKillPoint = Managers.Game.TotalKill;
        string playerSpriteName = Managers.Game.PlayerSpriteNames[0];

        List<RankData> allRanks = new List<RankData>(Managers.Data.RankDic.Values);

        RankData playerRankData = new RankData
        {
            name = playerName,
            KillPoint = playerKillPoint,
            CustomIndex = -1
        };
        allRanks.Add(playerRankData);

        List<RankData> sortedRanks = allRanks
            .OrderByDescending(r => r.KillPoint)
            .ThenBy(r => r.CustomIndex == -1 ? 0 : 1)
            .ThenBy(r => r.CustomIndex)
            .ToList();

        if (sortedRanks.Count == 0) return;

        int playerRank = sortedRanks.FindIndex(r => r.CustomIndex == -1) + 1;

        GetText((int)Texts.PlayerRankTextValue).text = playerRank.ToString();
        GetText((int)Texts.PlayerKillPointValue).text = playerKillPoint.ToString();

        string name = playerSpriteName.Replace("_Head", "");
        GetImage((int)Images.PlayerImage).sprite =
            Managers.Resource.Load<Sprite>(name);

        int equippedID = Managers.Game.EquippedAccessoryIndex;
        AccessoryData playerAccessory = Managers.Data.AccessoryDic[equippedID];
        GetImage((int)Images.PlayerAC).sprite = Managers.Resource.Load<Sprite>(playerAccessory.SpriteName);

        GetText((int)Texts.PlayerText).text = "플레이어";

        List<CustomData> customList = Managers.Data.CustomDic.Values.ToList();
        List<AccessoryData> accessoryList = Managers.Data.AccessoryDic.Values.ToList();

        // 1등 표시
        if (sortedRanks.Count >= 1)
        {
            RankData rank1 = sortedRanks[0];
            GetText((int)Texts.Rank1NameTextValue).text = rank1.name;
            GetText((int)Texts.Rank1KillTextValue).text = rank1.KillPoint.ToString();

            if (rank1.CustomIndex == -1)
            {
                // 플레이어면 플레이어 스프라이트와 악세사리
                GetImage((int)Images.Rank1PlayerImage).sprite =
                    Managers.Resource.Load<Sprite>(name);
                GetImage((int)Images.Rank1PlayerAC).sprite =
                    Managers.Resource.Load<Sprite>(playerAccessory.SpriteName);
            }
            else
            {
                // AI면 랜덤 스프라이트와 랜덤 악세사리
                CustomData randomCustom1 = customList[Random.Range(0, customList.Count)];
                GetImage((int)Images.Rank1PlayerImage).sprite =
                    Managers.Resource.Load<Sprite>(randomCustom1.SpriteName.Replace("_Head", ""));

                AccessoryData randomAccessory1 = accessoryList[Random.Range(0, accessoryList.Count)];
                GetImage((int)Images.Rank1PlayerAC).sprite =
                    Managers.Resource.Load<Sprite>(randomAccessory1.SpriteName);
            }
        }

        // 2등 표시
        if (sortedRanks.Count >= 2)
        {
            RankData rank2 = sortedRanks[1];
            GetText((int)Texts.Rank2NameTextValue).text = rank2.name;
            GetText((int)Texts.Rank2KillTextValue).text = rank2.KillPoint.ToString();

            if (rank2.CustomIndex == -1)
            {
                // 플레이어면 플레이어 스프라이트와 악세사리
                GetImage((int)Images.Rank2PlayerImage).sprite =
                    Managers.Resource.Load<Sprite>(name);
                GetImage((int)Images.Rank2PlayerAC).sprite =
                    Managers.Resource.Load<Sprite>(playerAccessory.SpriteName);
            }
            else
            {
                // AI면 랜덤 스프라이트와 랜덤 악세사리
                CustomData randomCustom2 = customList[Random.Range(0, customList.Count)];
                GetImage((int)Images.Rank2PlayerImage).sprite =
                    Managers.Resource.Load<Sprite>(randomCustom2.SpriteName.Replace("_Head", ""));

                AccessoryData randomAccessory2 = accessoryList[Random.Range(0, accessoryList.Count)];
                GetImage((int)Images.Rank2PlayerAC).sprite =
                    Managers.Resource.Load<Sprite>(randomAccessory2.SpriteName);
            }
        }

        // 3등부터 스크롤뷰에 추가
        GameObject container = GetObject((int)GameObjects.RankInfoScrollContentObject);
        container.DestroyChilds();

        for (int i = 2; i < sortedRanks.Count; i++)
        {
            RankData rankData = sortedRanks[i];
            UI_RankInfo rank = Managers.UI.MakeSubItem<UI_RankInfo>(container.transform);

            string spriteName;
            string accessoryName;

            if (rankData.CustomIndex == -1)
            {
                // 플레이어
                spriteName = playerSpriteName;
                accessoryName = playerAccessory.SpriteName;
            }
            else
            {
                // AI - 랜덤 스프라이트와 악세사리
                CustomData randomCustom = customList[Random.Range(0, customList.Count)];
                spriteName = randomCustom.SpriteName;

                AccessoryData randomAccessory = accessoryList[Random.Range(0, accessoryList.Count)];
                accessoryName = randomAccessory.SpriteName;
            }

            rank.SetInfo(rankData, _scrollrect, spriteName, accessoryName, i + 1);
        }
    }

    public override bool Init()
    {
        if (!base.Init()) return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        _scrollrect = GetObject((int)GameObjects.RankGroup).GetComponent<ScrollRect>();
        GetObject((int)GameObjects.BackgroundButton).BindEvent(OnClickCloseButton);
        return true;
    }

    private void OnClickCloseButton()
    {
        gameObject.SetActive(false);
        Managers.Sound.Play(Define.ESound.Effect, "BackButton");
    }
}