using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsAnimationManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] PlayerController playerController = null;
    [SerializeField] Animator leftArm = null;
    [SerializeField] Animator rightArm = null;

    [SerializeField] int numPossibleSwings = 3;
    private int _lastIndex = 0;

    private void Update()
    {
        if (playerController.isRunning)
            Run();
        else
            Idle();
    }

    public void Attack()
    {
        SetAttackBools();
        //SetRunBools(false);

        var index = Random.Range(0, numPossibleSwings);
        if (index == _lastIndex)
            index = (index + 1) % numPossibleSwings;
        _lastIndex = index;

        rightArm.SetInteger("SwipeIndex", index);
    }

    public void Run()
    {
        SetRunBools(true);
    }

    public void Idle()
    {
        SetRunBools(false);
    }

    public void Pickup()
    {
        leftArm.SetBool("Gun", true);
        leftArm.SetTrigger("Pickup");
    }

    public void Shoot()
    {
        leftArm.SetBool("Shoot", true);
    }


    void SetAttackBools()
    {
        rightArm.SetBool("Decision", true);
        leftArm.SetBool("Decision", true);
    }

    void SetRunBools(bool yn)
    {
        rightArm.SetBool("Run", yn);
        leftArm.SetBool("Run", yn);
    }

    public void DiedWithGun()
    {
        leftArm.SetTrigger("DiedWithGun");
    }
}
