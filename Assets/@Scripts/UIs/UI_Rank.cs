using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public class UI_Rank : UI_Scene
{
    TMP_Text[] rankNumberTexts = new TMP_Text[5];
    TMP_Text[] nameTexts = new TMP_Text[5];
    TMP_Text[] scoreTexts = new TMP_Text[5];

    // Player 전용 Row
    TMP_Text playerRankNumberText;
    TMP_Text playerRankNameText;
    TMP_Text playerRankScoreText;
    GameObject playerRankRow;

    float _rankUpdateTimer = 0f;
    int _prevPlayerRank = 999;
    PlayerController _firstPlace;

    Creature[] top = new Creature[5];
    int[] topScore = new int[5];

    Color rankNormal = Color.white;
    Color rankPlayer = new Color(1f, 0.92f, 0.3f);
    Color rankFirst = new Color(1f, 0.3f, 0.3f);

    #region Enum
    enum GameObjects
    {
        FX_UIEffect,
        RankObject,
        ContentObject,
        PlayerRankRow
    }

    enum Buttons { }

    enum Texts
    {
        Rank1Number, Rank1Name, Rank1Score,
        Rank2Number, Rank2Name, Rank2Score,
        Rank3Number, Rank3Name, Rank3Score,
        Rank4Number, Rank4Name, Rank4Score,
        Rank5Number, Rank5Name, Rank5Score,

        PlayerRankNumber,
        PlayerRankName,
        PlayerRankScore
    }

    enum Images { }
    #endregion

    public override bool Init()
    {
        if (!base.Init())
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetObject((int)GameObjects.FX_UIEffect).SetActive(false);

        InitRank();
        InitPlayerRankRow();
        return true;
    }

    private void Awake()
    {
        Init();
    }

    void Start()
    {
        //  처음엔 완전히 숨기기
        GetObject((int)GameObjects.RankObject).SetActive(false);

        StartCoroutine(Co_DelayedStart());
    }

    IEnumerator Co_DelayedStart()
    {
        //  AI 초기 스폰 완료 대기
        yield return new WaitUntil(() => !AISpawner.IsInitialSpawn);

        //  추가로 0.5초 대기 (안전)
        //yield return new WaitForSeconds(0.5f);

        //  랭킹 업데이트
        UpdateRankUI();

        //  이제 켜고 애니메이션
        GetObject((int)GameObjects.RankObject).SetActive(true);
        Util.PlayUIEnter(GetObject((int)GameObjects.RankObject), new Vector2(500, 0), 1);
    }

    void Update()
    {
        if (Managers.Object.Player == null || Managers.Object.Player.parent.isDead)
            return;

        _rankUpdateTimer += Time.deltaTime;
        if (_rankUpdateTimer >= 1f)
        {
            _rankUpdateTimer = 0f;
            UpdateRankUI();
        }
    }

    // TOP5 RankRow 설정
    void InitRank()
    {
        for (int i = 0; i < 5; i++)
        {
            int baseIndex = i * 3;

            rankNumberTexts[i] = GetText((int)Texts.Rank1Number + baseIndex);
            nameTexts[i] = GetText((int)Texts.Rank1Name + baseIndex);
            scoreTexts[i] = GetText((int)Texts.Rank1Score + baseIndex);

            rankNumberTexts[i].text = $"{i + 1}#";
        }
    }

    // Player 전용 RankRow 설정
    void InitPlayerRankRow()
    {
        playerRankRow = GetObject((int)GameObjects.PlayerRankRow);

        playerRankNumberText = GetText((int)Texts.PlayerRankNumber);
        playerRankNameText = GetText((int)Texts.PlayerRankName);
        playerRankScoreText = GetText((int)Texts.PlayerRankScore);

        playerRankRow.SetActive(false);
    }

    int _playerRank;
    public int PlayerRank => _playerRank;

    void UpdateRankUI()
    {
        var creatures = Managers.Object.Creatures;

        //  안전장치
        if (creatures == null || creatures.Count == 0)
            return;

        if (Managers.Object.Player == null || Managers.Object.Player.parent.isDead)
            return;

        int maxRank = 5;

        _firstPlace = null;

        for (int i = 0; i < creatures.Count; i++)
        {
            Creature c = creatures[i];
            if (c != null && !c.isDead &&
                (_firstPlace == null || c.points > _firstPlace.parent.points))
            {
                _firstPlace = c._head;
            }
        }

        for (int i = 0; i < maxRank; i++)
        {
            top[i] = null;
            topScore[i] = -1;
        }

        Creature player = Managers.Object.Player.parent;
        int playerScore = player.points;
        int order = 1;

        foreach (var c in creatures)
        {
            if (c == null || c.isDead) continue;

            int score = c.points;

            if (score > playerScore)
                order++;

            for (int r = 0; r < maxRank; r++)
            {
                if (score > topScore[r])
                {
                    for (int k = maxRank - 1; k > r; k--)
                    {
                        topScore[k] = topScore[k - 1];
                        top[k] = top[k - 1];
                    }

                    topScore[r] = score;
                    top[r] = c;
                    break;
                }
            }
        }

        _playerRank = order;
        Managers.Game.PlayerRank = order;

        Color normal = rankNormal;
        Color playerColor = rankPlayer;
        Color firstColor = rankFirst;

        bool playerInTop5 = false;

        // TOP5 출력
        for (int r = 0; r < maxRank; r++)
        {
            TMP_Text numTxt = rankNumberTexts[r];
            TMP_Text nameTxt = nameTexts[r];
            TMP_Text scoreTxt = scoreTexts[r];

            if (top[r] == null)
            {
                nameTxt.text = "-";
                scoreTxt.text = "";
                nameTxt.color = normal;
                scoreTxt.color = normal;
                continue;
            }

            Creature c = top[r];
            nameTxt.text = c._name;
            scoreTxt.text = topScore[r].ToString();

            if (c == player)
            {
                nameTxt.color = playerColor;
                scoreTxt.color = playerColor;
                playerInTop5 = true;
            }
            else
            {
                nameTxt.color = normal;
                scoreTxt.color = normal;
            }

            if (r == 0)
            {
                nameTxt.color = firstColor;
                scoreTxt.color = firstColor;
            }
        }

        // PlayerRankRow 처리
        if (playerInTop5)
        {
            playerRankRow.SetActive(false);
        }
        else
        {
            playerRankRow.SetActive(true);

            playerRankNumberText.text = _playerRank + "#";
            playerRankNameText.text = Managers.Game.PlayerName;
            playerRankScoreText.text = playerScore.ToString();

            playerRankNumberText.color = playerColor;
            playerRankNameText.color = playerColor;
            playerRankScoreText.color = playerColor;
        }

        bool enteredTop5 = (_prevPlayerRank > _playerRank) && (_playerRank <= 5);
        if (enteredTop5)
            StartCoroutine(PlayRankUpEffectDelayed(_playerRank - 1));

        _prevPlayerRank = _playerRank;
    }

    IEnumerator PlayRankUpEffectDelayed(int rankIndex)
    {
        yield return null;

        TMP_Text target = nameTexts[rankIndex];
        if (target == null) yield break;

        GameObject fx = GetObject((int)GameObjects.FX_UIEffect);
        if (fx == null) yield break;

        RectTransform fxRT = fx.GetComponent<RectTransform>();
        fxRT.position = target.transform.position;

        fx.SetActive(false);
        fx.SetActive(true);
    }

    public void BlindRank()
    {
        gameObject.SetActive(false);
    }
}