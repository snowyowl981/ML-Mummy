using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MummyILAgent : Agent
{
    private Transform tr;
    private Rigidbody rb;

    public float moveSpeed = 1.5f;
    public float turnSpeed = 200.0f;

    private StageManagerIL stageManager;
    private Renderer floorRd;
    private Material originMt;

    public Material goodMt, badMt;

    public override void Initialize()
    {
        MaxStep = 2000;
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();

        floorRd = tr.parent.Find("Floor").GetComponent<MeshRenderer>();
        originMt = floorRd.material;

        stageManager = transform.parent.GetComponent<StageManagerIL>();
    }

    public override void OnEpisodeBegin()
    {
        stageManager.InitStage();
        // 물리력 초기화
        rb.velocity = rb.angularVelocity = Vector3.zero;
        // Agent의 위치를 초기화
        tr.localPosition = new Vector3(0, 0.05f, -3.5f);
        tr.localRotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var action = actions.DiscreteActions;
        //Debug.Log($"[0] = {action[0]}, [1] = {action[1]}");

        Vector3 dir = Vector3.zero;
        Vector3 rot = Vector3.zero;

        // Branch 0 정지/전진/후진
        switch (action[0])
        {
            case 0: dir = Vector3.zero; break;
            case 1: dir = tr.forward; break;
            case 2: dir = -tr.forward; break;
        }
        // Branch 1 왼/오른쪽으로 회전
        switch (action[1])
        {
            case 1: rot = -tr.up; break;
            case 2: rot = tr.up; break;
        }

        tr.Rotate(rot, Time.fixedDeltaTime * turnSpeed);
        rb.AddForce(dir * moveSpeed, ForceMode.VelocityChange);

        // 마이너스 패널티를 적용
        AddReward(-1 / (float)MaxStep); //-0.005        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions; // 이산 (-1.0, 0.0, +1.0)

        actions.Clear();

        // Branch 0 - 이동
        // 정지/전진/후진  Non/W/S (0,1,2) : Branch 0의 Size 3
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
        if (coll.gameObject.name == "Floor") return;

        if (coll.collider.tag == stageManager.hintColor.ToString())
        {
            SetReward(+1.0f);
            EndEpisode();

            StartCoroutine(RevertMaterial(goodMt));
        }
        else
        {
            if (coll.collider.CompareTag("WALL") || coll.gameObject.name == "Hint")
            {
                SetReward(-0.05f);
            }
            else
            {
                SetReward(-1.0f);
                EndEpisode();

                StartCoroutine(RevertMaterial(badMt));
            }
        }
    }

    IEnumerator RevertMaterial(Material changeMt)
    {
        floorRd.material = changeMt;
        yield return new WaitForSeconds(0.2f);
        floorRd.material = originMt;
    }
}