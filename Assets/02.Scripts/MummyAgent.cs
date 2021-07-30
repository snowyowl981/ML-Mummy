using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;   // ML-Agents 네임스페이스
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/*
    에이전트의 역할
    1. 주변환경을 관층(Observations)
    2. 정책에 의해 행동(Action)
    3. 보상(Reward)을 받는다
*/

public class MummyAgent : Agent
{
    private Rigidbody rb;
    private Transform tr;
    private Transform targetTr;

    public Material goodMt;
    public Material badMt;
    private Material originMt;

    private Renderer floorRd;

    // 초기화 메소드
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        tr = GetComponent<Transform>();
        targetTr = tr.parent.Find("Target").GetComponent<Transform>();
        floorRd = tr.parent.Find("Floor")?.GetComponent<Renderer>();
        originMt = floorRd.material;
    }

    // 에피소드(학습의 단위)가 시작될때마다 호출되는 메소드
    public override void OnEpisodeBegin()
    {
        // 물리력 초기화
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 에이전트의 위치를 불규칙하게 변경
        tr.localPosition = new Vector3(Random.Range(-4.0f, 4.0f), 0.05f, Random.Range(-4.0f, 4.0f));

        // 타겟의 위치 변경
        targetTr.localPosition = new Vector3(Random.Range(-4.0f, 4.0f), 0.55f, Random.Range(-4.0f, 4.0f));

        StartCoroutine(RevertMaterialCoroutine());
    }

    IEnumerator RevertMaterialCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        floorRd.material = originMt;
    }

    // 주변 환경을 관측하는 콜백 메소드
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(targetTr.localPosition);  // (x, y, z)
        sensor.AddObservation(tr.localPosition);        // (x, y, z)
        sensor.AddObservation(rb.velocity.x);           // 1
        sensor.AddObservation(rb.velocity.y);           // 1
    }

    // 정책으로부터 전달받은 데이터를 기반으로 행동을 실행하는 메소드
    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.ContinuousActions;
        // [0] Up, Down
        // [1] Left, Right
        Vector3 dir = (Vector3.forward * action[0]) + (Vector3.right * action[1]);
        rb.AddForce(dir.normalized * 50.0f);

        // 마이너스 패널티
        SetReward(-0.001f);
    }

    // 개발자의 테스트용도/ 모방학습
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.ContinuousActions;
        action[0] = Input.GetAxis("Vertical");
        action[1] = Input.GetAxis("Horizontal");
    }

    // 보상 처리 로직
    void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("DEAD_ZONE"))
        {
            floorRd.material = badMt;

            SetReward(-1.0f);
            EndEpisode();   // 학습 종료
        }

        if (coll.collider.CompareTag("TARGET"))
        {
            floorRd.material = goodMt;
            
            SetReward(+1.0f);
            EndEpisode();
        }
    }
}
