using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnLight : MonoBehaviour
{
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
