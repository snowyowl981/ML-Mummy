using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManagerIL : MonoBehaviour
{
    public enum HINT_COLOR
    {
        BLACK, BLUE, GREEN, RED
    }

    // 힌트의 색상
    public HINT_COLOR hintColor = HINT_COLOR.BLACK;

    public Material[] hintMt;
    public string[] hintTag;

    private Renderer renderer;
    // 바로 전에 나왔던 색상을 저장할 변수
    private int prevTag = -1;

    // Start is called before the first frame update
    void Start()
    {
        renderer = transform.Find("Hint").GetComponent<Renderer>();
    }

    public void InitStage()
    {
        int idx = 0;

        do 
        {
            idx = Random.Range(0, hintMt.Length);
        } while (idx == prevTag);
        prevTag = idx;

        // 머티리얼 교체
        renderer.material = hintMt[idx];
        // Hint의 태그를 지정
        renderer.gameObject.tag = hintTag[idx];

        // 목표 타겟의 색상을 지정
        hintColor = (HINT_COLOR)idx;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            InitStage();
        }
    }
}
