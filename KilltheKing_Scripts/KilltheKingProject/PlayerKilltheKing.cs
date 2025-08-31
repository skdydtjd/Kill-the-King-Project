using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상태 변수 추가
public enum Playertype
{
    human,
    ai
}

[System.Serializable]
public class PlayerKilltheKing
{
    public Playertype type = Playertype.ai;
    public int playerNum;
    public SlotDefKilltheKing handSlotDef;
    public List<CardKilltheKing> hand;
    
    public CardKilltheKing AddCard(CardKilltheKing eCB)
    {
        if(hand == null)
        {
            hand = new List<CardKilltheKing>();
        }

        hand.Add(eCB);

        eCB.SetSortingLayerName("10");
        eCB.eventualSortLayer = handSlotDef.layerName;

        Fanhand();
        return eCB;
    }

    public CardKilltheKing RemoveCard(CardKilltheKing cardKilltheKing)
    {
        if(hand == null || !hand.Contains(cardKilltheKing))
        {
            return null;
        }

        hand.Remove(cardKilltheKing);
        Fanhand();
        return cardKilltheKing;
    }

    // 덱 모양 변경
    public void Fanhand()
    {
        Vector3 pos;

        for(int i = 0; i < hand.Count; i++)
        {
            pos = new Vector3(0, 0, 0);

            pos += handSlotDef.pos;
            pos.x = -12f + (6*i);

            if (KilltheKing.S.phase != TurnPhase.idle)
            {
                hand[i].timeStart = 0;
            }

            hand[i].MoveTo(pos);
            hand[i].state = CardState.toHand;

            hand[i].faceUp = (type == Playertype.human);

            // 4에서 2로 낮춤
            hand[i].eventualSortOrder = i * 2;
        }
    }

    // 카드 합이 적을 시 한 장 바꾸기 (AI)
    public void AICardChanging()
    {
        Utils.tr("ChangeCard");

        KilltheKing.S.phase = TurnPhase.waiting;

        CardKilltheKing cg;

        List<CardKilltheKing> ChangeCards = new List<CardKilltheKing>();

        foreach (var tCB in hand)
        {
            if (tCB.rank == KilltheKing.S.AIMinCard())
            {
                ChangeCards.Add(tCB);
            }
        }

        cg = ChangeCards[Random.Range(0, ChangeCards.Count)];
        RemoveCard(cg);

        KilltheKing.S.ChangeCard(cg);

        cg = AddCard(KilltheKing.S.Draw());

        KilltheKing.S.clip2.PlayDelayed(0.4f);

        cg.callbackPlayer = this;
    }

    // (플레이어)
    public void PlayerCardChanging()
    {
        Utils.tr("ChangeCard");

        KilltheKing.S.phase = TurnPhase.waiting;

        CardKilltheKing cg;

        List<CardKilltheKing> ChangeCards = new List<CardKilltheKing>();

        foreach (var tCB in hand)
        {
            if (tCB.rank == KilltheKing.S.PlayerMinCard())
            {
                ChangeCards.Add(tCB);
            }
        }

        cg = ChangeCards[Random.Range(0, ChangeCards.Count)];
        RemoveCard(cg);

        KilltheKing.S.ChangeCard(cg);

        cg = AddCard(KilltheKing.S.Draw());

        KilltheKing.S.clip2.PlayDelayed(0.4f);

        cg.callbackPlayer = this;
    }

    public void TakeTurn()
    {
        Utils.tr("AI.TakeTurn");

        if (type == Playertype.human) 
            return;

        KilltheKing.S.phase = TurnPhase.waiting;

        CardKilltheKing cb;

        List<CardKilltheKing> validCards = new List<CardKilltheKing>();

        // AI TakeTurn 새로 만든 조건으로 변경
        foreach (var tCB in hand)
        {
            if (KilltheKing.S.ValidPlay(tCB))
            {
                validCards.Add(tCB);
            }
        }

        cb = validCards[Random.Range(0,validCards.Count)];
        RemoveCard(cb);
        
        KilltheKing.S.TargettoAi(cb);

        cb.callbackPlayer = this;

        //
    }

    public void CBCallback(CardKilltheKing tCB)
    {
        Utils.tr("Player.CBCallback()", tCB.name, "Player " + playerNum);
        KilltheKing.S.PassTurn();
    }
}
