using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnLight : MonoBehaviour
{
    // 턴 진행 시 빛의 위치만 바꿔줌
    void Update()
    {
        transform.position = Vector3.back * 3;

        if(KilltheKing.CURRENT_PLAYER == null)
        {
            return;
        }

        transform.position += KilltheKing.CURRENT_PLAYER.handSlotDef.pos;
    }
}
