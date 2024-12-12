using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.Jobs;

public class MyAgent : Agent
{
    public GameObject Tree1;
    public GameObject Tree2;
    public GameObject Tree3;
    public GameObject Tree4;
    public GameObject Fence1;
    public GameObject Fence2;
    public GameObject Fence3;
    public GameObject Log1;
    public GameObject Log2;

    public GameObject Chicken;

    public float m_speed = 10;
    public int obstacleCount = 1;
    private Vector3 startPosition = new Vector3(0, 0, -40);

    private List<GameObject> obstaclePrefabs;
    private List<GameObject> obstacles = new List<GameObject>();

    private float previousDistanceToChicken;

    private enum ACTIONS
    {
        FORWARD = 0,
        BACKWARD = 1,
        RIGHT = 2,
        LEFT = 3,
    }

    private void Start()
    {
        obstaclePrefabs = new List<GameObject> { Tree1, Tree2, Tree3, Tree4, Fence1, Fence2, Fence3, Log1, Log2 };

        obstacles.AddRange(obstaclePrefabs);
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = startPosition;

        ResetObstacles();
        ResetChicken();

        previousDistanceToChicken = Vector3.Distance(transform.localPosition, Chicken.transform.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        float distanceToChicken = Vector3.Distance(transform.localPosition, Chicken.transform.localPosition);
        sensor.AddObservation(distanceToChicken);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionTaken = actions.DiscreteActions[0];

        switch (actionTaken)
        {
            case (int)ACTIONS.FORWARD:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case (int)ACTIONS.BACKWARD:
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case (int)ACTIONS.LEFT:
                transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
            case (int)ACTIONS.RIGHT:
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
        }

        transform.Translate(Vector3.forward * m_speed * Time.fixedDeltaTime);

        float distanceToChicken = Vector3.Distance(transform.localPosition, Chicken.transform.localPosition);
        if (distanceToChicken < previousDistanceToChicken)
        {
            AddReward(0.01f);
        }
        else
        {
            AddReward(-0.02f);
        }
        previousDistanceToChicken = distanceToChicken;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Obstacle")
        {
            AddReward(-8.0f);
            Debug.Log("Hit Obstacle");
            EndEpisode();
        }
        else if (other.tag == "Wall")
        {
            AddReward(-10.0f);
            Debug.Log("Hit Wall");
            EndEpisode();
        }
        else if (other.tag == "Chicken")
        {
            AddReward(10.0f);
            Debug.Log("Reached Chicken");
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut.Clear();

        discreteActionsOut[0] = (int)ACTIONS.FORWARD;

        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = (int)ACTIONS.FORWARD;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = (int)ACTIONS.BACKWARD;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = (int)ACTIONS.LEFT;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = (int)ACTIONS.RIGHT;
        }
    }

    private void ResetObstacles()
    {
        foreach (var obstacle in obstaclePrefabs)
        {
            obstacle.transform.localPosition = new Vector3(
                Random.Range(-20.0f, 20.0f),
                0,
                Random.Range(-30.0f, 30.0f)
            );
        }

        foreach (var obstacle in obstacles)
        {
            if (!obstaclePrefabs.Contains(obstacle))
            {
                Destroy(obstacle);
            }
        }
        obstacles.Clear();
        obstacles.AddRange(obstaclePrefabs);

        foreach (var prefab in obstaclePrefabs)
        {
            PlaceObstacles(prefab, obstacleCount);
        }
    }

    private void ResetChicken()
    {
        float range = 20.0f;
        Vector3 randomPosition = new Vector3(
            Random.Range(-range, range),
            0,
            Random.Range(35, 45)
        );
        Chicken.transform.localPosition = randomPosition;
    }

    private void PlaceObstacles(GameObject prefab, int count)
    {
        float rangeX = 20.0f;
        float rangeZ = 30.0f;

        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-rangeX, rangeX),
                0,
                Random.Range(-rangeZ, rangeZ)
            );
            GameObject obstacle = Instantiate(prefab, randomPosition, Quaternion.identity);
            obstacles.Add(obstacle);
        }
    }
}
