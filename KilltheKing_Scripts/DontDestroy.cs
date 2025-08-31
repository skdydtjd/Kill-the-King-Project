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

// 히어라키 창에 빈 오브젝트를 만들어 따로 AudioSouce컴포넌트를 할당 시켜주었으며 씬이 리로드되도 배경음은 계속 이어질 수 있도록 스크립트를 짬

// musics 게임오브젝트 변수에 Music 이란 태그를 달은 게임오브젝트를 찾아 할당되도록 FindGameObjectsWithTag를 씀 (tag는 따로 추가시켜 주었음)

// DontDestroyOnLoad 함수는 씬이 넘어가도 괄호안에 할당된 오브젝트를 다른 지역으로 이동하여 파괴하지 않도록 함

// 그러나 씬이 계속 리로드 될 경우 중복된 오브젝트가 계속 이동하면서 쌓이게 되어 노래가 도돌이표처럼 계속 중복(중첩) 플레이가 되게 되므로 개수가 2이상이 넘어가게 되면 Destroy하여 1개만 있을 수 있도록 유지함

// 속성 창 AudioClip에 배경음 리소스를 할당

// 항상 플레이 되고 있어야 하므로 play on awake, 노래가 끝나면 반복되도록 loop를 체크

// (보통 AudioSource 컴포넌트를 추가하면 play on awak는 항상 체크되어 있음)

// KilltheKing에 있는 여러 효과음 들은 play on awake, loop를 모두 체크해제하고 조건을 만족할 시에만 나오게 함

// 실행 후 히어라키 창을 보면 따로 생성된 DontDestroyOnLoad 씬에 들어가 파괴되지 않고 있음을 확인 할 수 있음
