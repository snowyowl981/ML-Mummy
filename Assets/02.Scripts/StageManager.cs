using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public GameObject goodItem;
    public GameObject badItem;

    public int goodItemCount = 30;
    public int badItemCount = 20;

    private List<GameObject> goodList = new List<GameObject>();
    private List<GameObject> badList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        SetStageObject();
    }

    public void SetStageObject()
    {
        // Good Item 생성
        for (int i=0; i<goodItemCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-23.0f, 23.0f),
                                      0.05f,
                                      Random.Range(-23.0f, 23.0f));

            Quaternion rot = Quaternion.Euler(Vector3.up * Random.Range(0, 360));

            // 스테이지를 여러개 생성하기 때문에 스테이지의 범위 내에서 아이템 좌표를 설정
            goodList.Add(Instantiate(goodItem, transform.position + pos, rot, transform));
        }

        // Bad Item 생성
        for (int i=0; i<badItemCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-23.0f, 23.0f),
                                      0.05f,
                                      Random.Range(-23.0f, 23.0f));

            Quaternion rot = Quaternion.Euler(Vector3.up * Random.Range(0, 360));

            // 스테이지를 여러개 생성하기 때문에 스테이지의 범위 내에서 아이템 좌표를 설정
            badList.Add(Instantiate(badItem, transform.position + pos, rot, transform));
        }
    }
}
