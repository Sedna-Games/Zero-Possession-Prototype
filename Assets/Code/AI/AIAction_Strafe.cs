using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAction_Strafe : AIAction
{
    [SerializeField] Vector2 strafeCooldown = new Vector2(0.5f, 1.5f);
    [SerializeField] Vector2 movementRange = new Vector2(5.0f, 10.0f);
    [Header("References")]
    [SerializeField] NavMeshAgent agent = null;
    [SerializeField] Animator animator = null;

    float _strafeCooldown = 0.0f;
    public override void SelectAction()
    {
        if (timeSinceLastSelected < _strafeCooldown)
            return;

        _strafeCooldown = AIBlackboard.RandomFloatHelper(strafeCooldown);
        NavMeshHit hit;
        var dist = AIBlackboard.RandomFloatHelper(movementRange);
        var direction = Random.Range(0, 101) >= 50 ? transform.right : -transform.right;


        animator.SetBool("StrafingRight", false);
        animator.SetBool("StrafingLeft", false);

        if (direction == transform.right)
            animator.SetBool("StrafingRight", true);
        else
            animator.SetBool("StrafingLeft", true);

        var destination = transform.position + direction * dist;
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!NavMesh.SamplePosition(destination, out hit, dist, NavMesh.AllAreas))
                destination = transform.position - AISensor.DirectionToPlayer(transform).normalized * dist;
            agent.SetDestination(destination);
            agent.updateRotation = true;
        }
        base.SelectAction();
    }

    public override void DeselectAction()
    {
        base.DeselectAction();
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(0.5f);
            animator.SetBool("StrafingRight", false);
            animator.SetBool("StrafingLeft", false);
        }
        StartCoroutine(Wait());
    }
}
