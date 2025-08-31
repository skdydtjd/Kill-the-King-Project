using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
    toDrawPile,
    drawPile,
    toHand,
    hand,
    toTarget,
    target,
    toDiscard,
    discard,
    to,
    idle
}

// 기존 Bartok으로 되어있던 변수 이름들을 KilltheKing변수에 맞게 이름 변경
public class CardKilltheKing : Card
{
    static public float MOVE_DURATION = 0.5f;
    static public string MOVE_EASING = Easing.InOut;
    static public float CARD_WIDTH = 2f;

    [Header("Set Dynamically: CardKilltheKing")]
    public CardState state = CardState.drawPile;
    public List<Vector3> bezierPts;
    public float timeStart, timeDuration;
    public int eventualSortOrder;
    public string eventualSortLayer;

    public GameObject reportFinishTo = null;

    [System.NonSerialized]
    public PlayerKilltheKing callbackPlayer = null;

    public void Move(Vector3 ePos, Quaternion eRot)
    {
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.position);
        bezierPts.Add(ePos);

        if (timeStart == 0)
        {
            timeStart = Time.time;
        }

        timeDuration = MOVE_DURATION;
        state = CardState.to;
    }

    // 덱을 분배 또는 정해진 위치로 이동 시킬 때 Bartok과 달리 회전 매개변수는 필요치 않으므로 제거
    public void MoveTo(Vector3 ePos)
    {
        Move(ePos, Quaternion.identity);
    }

    void Update()
    {
        switch (state)
        {
            case CardState.toHand:
            case CardState.toTarget:
            case CardState.toDrawPile:
            case CardState.toDiscard:
            case CardState.to:
                float u = (Time.time - timeStart) / timeDuration;
                float uC = Easing.Ease(u, MOVE_EASING);

                if(u < 0)
                {
                    transform.localPosition = bezierPts[0];
                    return;
                }
                else if (u >= 1)
                {
                    uC = 1;

                    if (state == CardState.toHand)
                    {
                        state = CardState.hand;
                    }
                    if (state == CardState.toTarget)
                    {
                        state = CardState.target;
                    }
                    if (state == CardState.toDrawPile)
                    {
                        state = CardState.drawPile;
                    }
                    if (state == CardState.to)
                    {
                        state = CardState.idle;
                    }
                    if(state == CardState.toDiscard)
                    {
                        state = CardState.discard;
                    }

                    transform.localPosition = bezierPts[bezierPts.Count - 1];

                    timeStart = 0;

                    if (reportFinishTo != null)
                    {
                        reportFinishTo.SendMessage("CBCallback", this);
                        reportFinishTo = null;
                    }
                    else if (callbackPlayer != null)
                    {
                        callbackPlayer.CBCallback(this);
                        callbackPlayer = null;
                    }
                }
                else
                {
                    Vector3 pos = Utils.Bezier(uC, bezierPts);
                    transform.localPosition = pos;

                    if(u > 0.5f)
                    {
                        SpriteRenderer sRend = spriteRenderers[0];

                        if(sRend.sortingOrder != eventualSortOrder)
                        {
                            SetSortOrder(eventualSortOrder);
                        }

                        if(sRend.sortingLayerName != eventualSortLayer)
                        {
                            SetSortingLayerName(eventualSortLayer);
                        }
                    }
                }
                break;
        }
    }

    public override void OnMouseUpAsButton()
    {
        KilltheKing.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
