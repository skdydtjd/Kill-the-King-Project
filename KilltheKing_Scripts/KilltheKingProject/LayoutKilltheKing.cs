using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDefKilltheKing
{
    public float x;
    public float y;

    public bool faceUp = false;

    public string layerName = "Defalut";
    public int layerID = 0;

    public int id;
    public float rot;

    public string type = "slot";

    public Vector2 stagger;

    public int player;

    public Vector3 pos;
}

public class LayoutKilltheKing : MonoBehaviour
{
    [Header("Set Dynamically")]
    public PT_XMLReader xMLReader;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;

    public List<SlotDefKilltheKing> slotDefs;
    public SlotDefKilltheKing drawPile;
    public SlotDefKilltheKing discardPile;

    // 변경하였던 Layout XML의 target 데이터를 읽어와서 할당할 객체 선언
    public SlotDefKilltheKing target1;
    public SlotDefKilltheKing target2;

    public void ReadLayout(string xmlText)
    {
        xMLReader = new PT_XMLReader();
        xMLReader.Parse(xmlText);
        xml = xMLReader.xml["xml"][0];

        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        SlotDefKilltheKing tSD;
        PT_XMLHashList slotX = xml["slot"];

        for(int i = 0; i < slotX.Count; i++)
        {
            tSD = new SlotDefKilltheKing();

            if (slotX[i].HasAtt("type"))
            {
                tSD.type = slotX[i].att("type");
            }
            else
            {
                tSD.type = "slot";
            }

            tSD.x = float.Parse(slotX[i].att("x"));
            tSD.y = float.Parse(slotX[i].att("y"));
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);

            tSD.layerID = int.Parse(slotX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();

            switch (tSD.type)
            {
                case "slot":
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotX[i].att("xstagger"));
                    drawPile = tSD;
                    break;

                case "discardpile":
                    discardPile = tSD;
                    break;

                case "target1": // target 읽어오기 (플레이어)
                    target1 = tSD;
                    break;

                case "target2": // target 읽어오기 (AI)
                    target2 = tSD;
                    break;

                case "hand":
                    tSD.player = int.Parse(slotX[i].att("player"));
                    tSD.rot = float.Parse(slotX[i].att("rot"));
                    slotDefs.Add(tSD);
                    break;
            }
        }
    }
}
