using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAction_Attack : AIAction
{
    [SerializeField] Vector2 aimLeading = Vector2.zero;//0.15
    [SerializeField] float turnSpeedWhileStandingStill = 2.5f;
    [SerializeField] float distanceTolerance = 5.0f;
    [SerializeField] float angleTolerance = 0.1f;
    [Tooltip("The distance at which the AI will shit itself.")]
    [SerializeField] float shitSelfDistance = 5.0f;

    [Header("References")]
    [SerializeField] WeaponManager weaponManager = null;
    [SerializeField] Gun gun = null;

    GameObject player = null;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PlayerCapsule");
    }

    public override void SelectAction()
    {
        if (!InsideIntersectionPoint(0, AISensor.PlayerPosition))
        {
            var gunDir = AISensor.DirectionToPlayer(gun.transform.parent);
            var newGunRotation = Quaternion.LookRotation(gunDir.normalized);
            AIBlackboard.RotationHelper(gun.transform.parent.rotation, newGunRotation, gun.transform.parent, turnSpeedWhileStandingStill);
            if (AISensor.DistanceToPlayer(transform) <= shitSelfDistance || Vector3.Angle(transform.forward,gun.transform.parent.forward) < angleTolerance)
                weaponManager.OnPrimaryWeapon();
        }
        else
            weaponManager.OnPrimaryWeapon();

        base.SelectAction();
    }

    public bool InsideIntersectionPoint(int index, Vector3 point)
    {
        var dist = (gun.GetBulletEndPoint(index) - point).magnitude;
        return dist <= distanceTolerance;
    }


}
