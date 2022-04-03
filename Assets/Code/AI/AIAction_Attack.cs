using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAction_Attack : AIAction
{
    [SerializeField] float timeBetweenShotBursts = 0.5f;
    [SerializeField] Vector2 shotAmount = new Vector2(1.0f, 3.0f);
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
    int _shotAmount = 0;
    int _shotCount = 0;
    Coroutine _resetShotCoroutine = null;
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
            if (AISensor.DistanceToPlayer(transform) <= shitSelfDistance || Vector3.Angle(transform.forward, gun.transform.parent.forward) < angleTolerance)
                Fire();
        }
        else
            Fire();

        base.SelectAction();
    }

    void Fire()
    {
        //reset shot count
        if (_shotCount >= _shotAmount)
        {
            IEnumerator ResetShotCount()
            {
                yield return new WaitForSeconds(timeBetweenShotBursts);

                _shotCount = 0;
                _shotAmount = Mathf.CeilToInt(AIBlackboard.RandomFloatHelper(shotAmount));
                _resetShotCoroutine = null;
            }
            if (_resetShotCoroutine == null)
                _resetShotCoroutine = StartCoroutine(ResetShotCount());
            return;
        }
        if (weaponManager.CanPrimaryAttack())
            _shotCount++;
        weaponManager.OnPrimaryWeapon();
    }

    public bool InsideIntersectionPoint(int index, Vector3 point)
    {
        var dist = (gun.GetBulletEndPoint(index) - point).magnitude;
        return dist <= distanceTolerance;
    }


}
