using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MummyRayAgent : Agent
{
    private Transform tr;
    private Rigidbody rb;
    private StageManager stageManager;

    public float moveSpeed = 1.5f;
    public float turnSpeed = 200.0f;

    private Renderer floorRd;
    public Material goodMt, badMt;
    private Material originMt;

    public override void Initialize()
    {

        // 한 에피소드(학습단위)당 시도 횟수
        // 이 횟수동안 아무 리워드도 획득하지 못하면 재시작
        MaxStep = 5000;

        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        stageManager = tr.parent.GetComponent<StageManager>();
        floorRd = tr.parent.Find("Floor").GetComponent<MeshRenderer>();
        originMt = floorRd.material;
    }

    // 아이템에 맞는 색으로 변경했다가 다시 원본색으로 환원
    IEnumerator RevertMaterialCoroutine(Material changeMt)
    {
        floorRd.material = changeMt;
        yield return new WaitForSeconds(0.2f);
        floorRd.material = originMt;
    }

    public override void OnEpisodeBegin()
    {
        // 스테이지 초기화
        stageManager.SetStageObject();

        // 물리엔진 초기화
        rb.velocity = rb.angularVelocity = Vector3.zero;

        // 에이전트의 위치를 변경
        tr.localPosition = new Vector3(Random.Range(-22.0f, 22.0f),
                                       0.05f,
                                       Random.Range(-22.0f, 22.0f));

        // 에이전트의 회전
        tr.localRotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions;
        Debug.Log($"[0] = {action[0]}, [1] = {action[1]}");

        Vector3 dir = Vector3.zero;
        Vector3 rot = Vector3.zero;

        // Branch 0 정지/전진/후진
        switch (action[0])
        {
            case 0: dir = Vector3.zero;
                break;

            case 1: dir = tr.forward; 
                break;

            case 2: dir = -tr.forward; 
                break;
        }

        // Branch 1 좌/우 회전
        switch (action[1])
        {
            case 1: rot = -tr.up;
                break;

            case 2: rot = tr.up;
                break;
        }

        tr.Rotate(rot, Time.fixedDeltaTime * turnSpeed);
        rb.AddForce(dir * moveSpeed, ForceMode.VelocityChange);

        // (제자리에 머물러있을 것에 대비) 마이너스 패널티를 적용
        AddReward(-1 / (float)MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;   // 이산 (-1.0, 0.0, +1.0)

        // Clear를 해줬기 때문에 키코드가 들어가지 않은 0상태는 0에 할당
        actions.Clear();

        // Branch 0 - 이동
        // 정지/전진/후진 Non/W/S (0,1,2) : Branch 0의 Size 3
        if (Input.GetKey(KeyCode.W))
        {
            actions[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            actions[0] = 2;
        }

        // Branch 1 - 회전
        // 정지/좌회전/우회전 Non/A/D (0,1,2) : Branch 1의 Size 3
        if (Input.GetKey(KeyCode.A))
        {
            actions[1] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            actions[1] = 2;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("GOOD_ITEM"))
        {
            // 충돌 시 물리력 초기화(GOOD_ITEM을 많이 먹어야 하므로 에피소드 종료는 안함)
            rb.velocity = rb.angularVelocity = Vector3.zero;
            Destroy(coll.gameObject);
            AddReward(+1.0f);

            StartCoroutine(RevertMaterialCoroutine(goodMt));
        }

        if (coll.collider.CompareTag("BAD_ITEM"))
        {
            floorRd.material = badMt;

            AddReward(-1.0f);
            EndEpisode();

            StartCoroutine(RevertMaterialCoroutine(badMt));
        }

        if (coll.collider.CompareTag("WALL"))
        {
            AddReward(-0.1f);
        }
    }
}
