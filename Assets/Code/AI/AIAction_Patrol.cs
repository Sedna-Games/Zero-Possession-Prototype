using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAction_Patrol : AIAction
{
    [Header("References")]
    [SerializeField] NavMeshAgent agent = null;
    [SerializeField] Vector2 startRadius = new Vector2(5.0f, 10.0f);

    Vector3 _startingPos = Vector3.zero;

    private void Start()
    {
        _startingPos = transform.position;
    }

    public override void SelectAction()
    {
        var radius = AIBlackboard.RandomFloatHelper(startRadius);

        var destination = new Vector3(Random.Range(0.0f, 1.0f), 0.0f, Random.Range(0.0f, 1.0f)).normalized * radius + _startingPos;

        agent.SetDestination(destination);
        base.SelectAction();
    }
}
