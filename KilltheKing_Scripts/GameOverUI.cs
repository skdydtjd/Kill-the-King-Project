using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public Text txt;

    // 매 라운드 진행 시 포인트가 Update에 의해 중복 계산 되지않도록 gameover 변수 추가
    bool gameOver = false;  

    void Awake()
    {
        txt = GetComponent<Text>();
        txt.text = "";
    }

    void Update()
    {
        if (KilltheKing.S.phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }

        if (KilltheKing.CURRENT_PLAYER == null) 
            return;

        // 승패 판정 시 포인트 계산, 출력 문구 (플레이어 승리 시)
        if (KilltheKing.CURRENT_PLAYER.type == Playertype.human && gameOver == false)
        {
            KilltheKing.Point2 -= 20;

            KilltheKing.S.PlayerPoint.text = "Point = " + KilltheKing.Point1.ToString();
            KilltheKing.S.AIPoint.text = "Point = " + KilltheKing.Point2.ToString();

            if (KilltheKing.Point2 <= 0)
            {
                txt.text = "Game Over" + "\n" + "You Finally Win";

                KilltheKing.Point1 = 100;
                KilltheKing.Point2 = 100;
            }
            else
            {
                txt.text = "You Win!";
            }

            gameOver = true;
        }
        else if (KilltheKing.CURRENT_PLAYER.type == Playertype.ai && gameOver == false) // AI 승리 시
        {
            KilltheKing.Point1 -= 20;

            KilltheKing.S.PlayerPoint.text = "Point = " + KilltheKing.Point1.ToString();
            KilltheKing.S.AIPoint.text = "Point = " + KilltheKing.Point2.ToString();

            if (KilltheKing.Point1 <= 0)
            {
                txt.text = "Game Over" + "\n" + "AI Finally Win";

                KilltheKing.Point1 = 100;
                KilltheKing.Point2 = 100;
 
            }
            else
            { 
                txt.text = "AI Win!";
            }

            gameOver = true;
        }
    }
}
