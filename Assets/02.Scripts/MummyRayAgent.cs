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

    public float moveSpeed = 1.5f;
    public float turnSpeed = 200.0f;

    public override void Initialize()
    {

        // 한 에피소드(학습단위)당 시도 횟수
        // 이 횟수동안 아무 리워드도 획득하지 못하면 재시작
        MaxStep = 5000;

        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // 스테이지 초기화

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
}
