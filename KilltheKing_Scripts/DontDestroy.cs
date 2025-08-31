using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 배경음을 위한 스크립트 추가
public class DontDestroy : MonoBehaviour
{
    private GameObject[] musics;

    void Awake()
    {
        musics = GameObject.FindGameObjectsWithTag("Music");

        if(musics.Length>=2)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(transform.gameObject);
    }
}
