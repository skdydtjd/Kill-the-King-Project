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

    // 카드 패 총합 변수
    public int total1 = 0;
    public int total2 = 0;

    // AI, 플레이어의 각 패의 총합
    public Text playerTotal;
    public Text AITotal;

    // 포인트 변수
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

    // 타켓이 하나였으나 여기선 AI, 플레이어 각각 2개이므로 list로 바꿈
    public List<CardKilltheKing> targetCards;

    public TurnPhase phase = TurnPhase.idle;

    private LayoutKilltheKing layout;
    private Transform layoutAnchor;

    void Awake()
    {
        S = this;
    }

    // 선언한 변수들 초기화
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

        // 플레이어
        players[0].type = Playertype.human;

        CardKilltheKing tCB;

        // 초기 배분되어질 카드 덱들의 수 변경
        for (int i = 0; i < numStartingCards; i++)
        {
            // 기존 4인 이던 것을 AI, 플레이어로 2인 변경
            for (int j = 0; j < 2; j++)
            {
                tCB = Draw();
                tCB.timeStart = Time.time + drawTimeStagger * (i * 2 + j);
                players[j].AddCard(tCB);

                // AI, 플레이어의 각 덱 총합 계산
                if (j == 0) // 플레이어
                {
                    total1 += players[0].hand[i].rank;
                }
                else if (j == 1) // AI
                {
                    total2 += players[1].hand[i].rank;
                }

                // Bartok에서 첫 Target카드까지 배치한 후 세팅이 끝났음을 알리는 DrawFirstTarget이라는 함수를 이용하여 reportFinishTo를 호출하였는데 
                // 여기선 첫 타겟이 없이 분배 후 바로 시작해도 상관없으므로 제거
                
                // (reportFinishTo를 for문 안에 넣고 AI를 먼저 턴을 잡아 행동하게 할 경우 세팅 도중 턴을 빠르게 넘겨버림)
                // (따라서 카드가 덱에 완전히 들어오기 전에 target으로 보내려는 충돌이 일어나 버그가 생기게 된다.)
                // (플레이어를 먼저 턴을 잡게 해줌으로서 해결)

                // 이에 따라 AI의 target으로 보내는 함수를 딜레이를 주어 늦게 실행되게 하는 방법도 있을 수 있으나 플레이어의 경우 카드를 클릭하기 전까지 대기하니 
                // 플레이어가 먼저 하게 바꿔주는 방법이 더 간편하다 생각하여 플레이어부터 시작하게 바꿔줌

            }
        }

        // 효과음 재생
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
        else // 합이 같을 경우 바로 게임 진행
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
        // 플레이어부터 시작할 수 있도록 인자값 변경
        PassTurn(0);
    }

    public void PassTurn(int num = -1)
    {
        if (num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);

            // PassTurn 함수에서는 기존에 4인이니 1~4로 인덱스 넘버를 주던 것을 2인으로 바꿔주었음
            // num = (ndx +1) % 4  ---> num = (ndx + 1) % 2 로 바꿈

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

    // AI가 카드 선택할 시 조건 함수
    // (카드 덱 중 A카드와 가장 높은 카드가 둘 다 있을 경우 랜덤으로 A 또는 가장 높은 카드 중 선택함)
    // (제일 높은 카드가 문양 별로 2~4개 가 있을 경우엔 그 카드들 중 가장 먼저 읽어들이게 되는 카드를 선택) - 추후 StrongNumber를 활용하여 진짜 가장 높은 카드를 선택할 수 있게 할 수 있음
    // (위의 상황을 종합하여 A카드와 높은 카드가 여러 개 있을 경우에도 A카드와 높은 카드 중 랜덤 선택)
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

    // 플레이어가 선택한 카드 이동
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

    // AI가 선택한 카드 이동
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
        // 플레이어의 덱이 아닌 AI를 덱이 클릭되지 않게하려고 한 것으로 보이나 밑에 더 확실한 조건을 추가하고 주석 처리함
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

            // 효과음 재생
            clip2.Play();
        }
    }

    // A가 K를 이길 수 있도록 하는 조건 중 하나
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

        // targetCards[0] = 플레이어, targetCards[1] = AI
        
        if (targetCards[0] != null && targetCards[1] != null)
        {
            // A와 K가 선택되었을 때 승패판정
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

            // 기존 숫자 크기 비교 승패판정
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
