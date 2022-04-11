using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAction_Attack : AIAction
{
    [SerializeField] float timeBetweenShotBursts = 0.5f;
    [SerializeField] Vector2 shotAmount = new Vector2(1.0f, 3.0f);
    [Range(0.0f,1.0f)]
    [SerializeField] float aimLeading = 0.1f;
    [Range(0.1f,1.0f)]
    [SerializeField] float turnSpeed = 0.8f;
    [SerializeField] float distanceTolerance = 5.0f;
    [SerializeField] float angleTolerance = 0.1f;
    [Tooltip("The distance at which the AI will shit itself.")]
    [SerializeField] float closeAimingDistance = 5.0f;

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
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, closeAimingDistance);
    }
#endif

    public override void SelectAction()
    {
        if (!InsideIntersectionPoint(0, AISensor.PlayerPosition))
        {
            var gunDir = Vector3.zero;
            if (AISensor.DistanceToPlayer(transform) <= closeAimingDistance)
                gunDir = AISensor.DirectionToPlayerNextPosition(gun.transform,aimLeading);
            else
                gunDir = AISensor.DirectionToPlayerNextPosition(gun.transform.parent,aimLeading);
            
            var newGunRotation = Quaternion.LookRotation(gunDir.normalized);
            AIBlackboard.RotationHelper(gun.transform.parent.rotation, newGunRotation, gun.transform.parent, turnSpeed);
            
            if (Vector3.Angle(transform.forward, gun.transform.parent.forward) < angleTolerance)
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
