using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public enum TurnPhase
{
    idle,
    pre,
    waiting,
    post,
    gameOver
}

public class KilltheKing : MonoBehaviour
{
    static public KilltheKing S;
    static public PlayerKilltheKing CURRENT_PLAYER;

    public int total1 = 0;
    public int total2 = 0;

    // AI, 플레이어의 각 패의 총합
    public Text playerTotal;
    public Text AITotal;

    public static int Point1 = 100;
    public static int Point2 = 100;

    // AI, 플레이어의 세트 스코어
    public Text AIPoint;
    public Text PlayerPoint;

    // 카드 숫자로 무승부 시 문양의 세기를 알리는 UI
    public Text AIStrongNumber;
    public Text PlayerStrongNumber;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;

    public Vector3 layoutCenter = Vector3.zero;

    public int numStartingCards = 5;
    public float drawTimeStagger = 0.1f;

    // 효과음(카드 분배, 카드 선정, 승리 판정)
    public AudioSource clip1;
    public AudioSource clip2;
    public AudioSource clip3;

    [Header("Set Dynamically")]
    public Deck deck;
    public List<CardKilltheKing> drawPile;
    public CardKilltheKing discard;

    public List<PlayerKilltheKing> players;

    // 타켓이 하나였으나 여기선 2개이므로 list로 바꿈
    public List<CardKilltheKing> targetCards;

    public TurnPhase phase = TurnPhase.idle;

    private LayoutKilltheKing layout;
    private Transform layoutAnchor;

    void Awake()
    {
        S = this;
    }

    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<LayoutKilltheKing>();
        layout.ReadLayout(layoutXML.text);

        drawPile = UpgradeCardList(deck.cards);

        layoutGame();

        playerTotal.text = "Total = " + total1.ToString();
        AITotal.text = "Total = " + total2.ToString();

        PlayerPoint.text = "Point = " + Point1.ToString();
        AIPoint.text = "Point = " + Point2.ToString();
    }

    List<CardKilltheKing> UpgradeCardList(List<Card> lCD)
    {
        List<CardKilltheKing> lCB = new List<CardKilltheKing>();

        foreach (var tCD in lCD)
        {
            lCB.Add(tCD as CardKilltheKing);
        }
        return lCB;
    }

    public void ArrangeDrawPile()
    {
        CardKilltheKing tCB;

        for (int i = 0; i < drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.SetParent(layoutAnchor);
            tCB.transform.localPosition = layout.drawPile.pos;

            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 2);
            tCB.state = CardState.drawPile;
        }
    }

    void layoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject gameobject = new GameObject("_LayoutAnchor");
            layoutAnchor = gameobject.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        ArrangeDrawPile();

        PlayerKilltheKing player;
        players = new List<PlayerKilltheKing>();

        foreach (var tSD in layout.slotDefs)
        {
            player = new PlayerKilltheKing();
            player.handSlotDef = tSD;
            players.Add(player);
            player.playerNum = tSD.player;
        }

        players[0].type = Playertype.human;

        CardKilltheKing tCB;

        for (int i = 0; i < numStartingCards; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                tCB = Draw();
                tCB.timeStart = Time.time + drawTimeStagger * (i * 2 + j);
                players[j].AddCard(tCB);

                if (j == 0)
                {
                    total1 += players[0].hand[i].rank;
                }
                else if (j == 1)
                {
                    total2 += players[1].hand[i].rank;
                }
            }
        }
        clip1.PlayDelayed(0.4f);

        Invoke("StartCardChange", 2f);
    }

    // 카드 바꾸기
    public void StartCardChange()
    {
        if (total1 > total2)
        {
            players[1].AICardChanging();
        }
        else if (total2 > total1)
        {
            players[0].PlayerCardChanging();
        }
        else
        {
            StartGame();
        }
    }

    public void CBCallback(CardKilltheKing cardKilltheKing)
    {
        Utils.tr("KilltheKing:CBCallback()", cardKilltheKing.name);
        Invoke("StartGame",1);
    }

    public void StartGame()
    {
        PassTurn(0);
    }

    public void PassTurn(int num = -1)
    {
        if (num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % 2;
        }

        int lastPlayerNum = -1;

        if (CURRENT_PLAYER != null)
        {
            lastPlayerNum = CURRENT_PLAYER.playerNum;

            if (CheckGameOver())
            {
              return;
            }
        }

        CURRENT_PLAYER = players[num];

        phase = TurnPhase.pre;

        CURRENT_PLAYER.TakeTurn();

        Utils.tr("KilltheKing:PassTurn", "Old: " + lastPlayerNum,
        "New: " + CURRENT_PLAYER.playerNum);
    }

    // AI changeCard 조건
    public int AIMinCard()
    {
        int standard = 13;

        for (int i = 0; i < players[1].hand.Count; i++)
        {
            if (players[1].hand[i].rank != 1)
            {
                if (standard > players[1].hand[i].rank)
                {
                    standard = players[1].hand[i].rank;
                }
            }
        }
        return standard;
    }

    // Player changeCard 조건
    public int PlayerMinCard()
    {
        int standard = 13;

        for (int i = 0; i < players[0].hand.Count; i++)
        {
            if (players[0].hand[i].rank != 1)
            {
                if (standard > players[0].hand[i].rank)
                {
                    standard = players[0].hand[i].rank;
                }
            }
        }
        return standard;
    }

    // AI targetCard 조건
    public int MaxCard()
    {
        int standard = 0;

        for (int i = 0; i < players[1].hand.Count; i++)
        {
            if (standard < players[1].hand[i].rank)
            {
                standard = players[1].hand[i].rank;
            }
        }

        return standard;
    }

    public bool ValidPlay(CardKilltheKing cardKilltheKing)
    {
        if (cardKilltheKing.rank == 1)
        {
            return true;
        }

        if (cardKilltheKing.rank == MaxCard())
        {
            return true;
        }

        return false;
    }

    public CardKilltheKing TargettoHuman(CardKilltheKing tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.target1.pos + Vector3.back);
        tCB.state = CardState.toTarget;
        tCB.faceUp = true;

        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target1.layerName;

        targetCards[0] = tCB;

        return tCB;
    }

    public CardKilltheKing TargettoAi(CardKilltheKing tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.target2.pos + Vector3.back);
        tCB.state = CardState.toTarget;
        tCB.faceUp = true;

        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target2.layerName;
        clip2.Play();

        targetCards[1] = tCB;

        return tCB;
    }

    // 카드 바꾸기 이동 모션
    public CardKilltheKing ChangeCard(CardKilltheKing tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CardState.toDiscard;
        tCB.faceUp = true;

        tCB.SetSortingLayerName("10");

        tCB.eventualSortLayer = layout.discardPile.layerName;

        discard = tCB;

        return tCB;
    }

    public CardKilltheKing Draw()
    {
        if (drawPile.Count == 0)
        {
            ArrangeDrawPile();

            float t = Time.time;

            foreach (var tCB in drawPile)
            {
                tCB.transform.localPosition = layout.drawPile.pos;
                tCB.callbackPlayer = null;
                tCB.MoveTo(layout.drawPile.pos);
                tCB.timeStart = t;
                t += 0.02f;
                tCB.state = CardState.toDrawPile;
                tCB.eventualSortLayer = "1";
            }
        }

        CardKilltheKing cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }

    public void CardClicked(CardKilltheKing tCB)
    {
        // 있어도 전혀 쓰이는 곳이 없으므로 주석 처리함
        /*
        if (CURRENT_PLAYER.type != Playertype.human)
            return;
        */
        if (phase == TurnPhase.waiting)
            return;

        // 기존의 카드 State가 hand인지만 체크하던 것과 달리 플레이어의 카드 덱만이 faceUp이 되어있는 것을 
        // 이용하여 조건을 추가함
        if (targetCards[0] == null && tCB.state == CardState.hand && tCB.faceUp == true)
        {
            CURRENT_PLAYER.RemoveCard(tCB);
            TargettoHuman(tCB);
            tCB.callbackPlayer = CURRENT_PLAYER;
            Utils.tr("KilltheKing:CardClicked()", "Play", tCB.name,
            targetCards[0].name + " is target");
            phase = TurnPhase.waiting;
            clip2.Play();
        }
    }

    public bool diff()
    {
        int diff = Mathf.Abs(targetCards[0].rank - targetCards[1].rank);

        if(diff == 12)
        {
            return true;
        }

        return false;
    }

    public bool CheckGameOver()
    {
        if (targetCards[0] != null && targetCards[1] != null)
        {
            if (targetCards[0].rank == 1 && diff())
            {
                CURRENT_PLAYER.type = Playertype.human;
                phase = TurnPhase.gameOver;
                clip3.Play();
                Invoke("RestartGame", 4);
                return true;
            }

            if (targetCards[1].rank == 1 && diff())
            {
                CURRENT_PLAYER.type = Playertype.ai;
                phase = TurnPhase.gameOver;
                clip3.Play();
                Invoke("RestartGame", 4);
                return true;
            }

            if (targetCards[0].rank > targetCards[1].rank)
            {
                CURRENT_PLAYER.type = Playertype.human;
                phase = TurnPhase.gameOver;
                clip3.Play();
                Invoke("RestartGame", 4);
                return true;
            }

            if (targetCards[0].rank < targetCards[1].rank)
            {
                CURRENT_PLAYER.type = Playertype.ai;
                phase = TurnPhase.gameOver;
                clip3.Play();
                Invoke("RestartGame", 4);
                return true;
            }

            // 족보를 통한 승패판정
            if (targetCards[0].rank == targetCards[1].rank)
            {
                AIStrongNumber.text = targetCards[1].suit + "\n" + "StrongNumber = " + targetCards[1].StrongNumber;
                PlayerStrongNumber.text = targetCards[0].suit + "\n" + "StrongNumber = " + targetCards[0].StrongNumber;

                if (targetCards[0].StrongNumber > targetCards[1].StrongNumber)
                {
                    CURRENT_PLAYER.type = Playertype.human;
                    phase = TurnPhase.gameOver;
                    clip3.Play();
                    Invoke("RestartGame", 4);
                    return true;
                }

                if(targetCards[0].StrongNumber < targetCards[1].StrongNumber)
                {
                    CURRENT_PLAYER.type = Playertype.ai;
                    phase = TurnPhase.gameOver;
                    clip3.Play();
                    Invoke("RestartGame", 4);
                    return true;
                }
            }
        }
        return false;
    }

    public void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("Kill_the_King");
    }
}
