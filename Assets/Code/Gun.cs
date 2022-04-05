using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class Gun : Weapon
{
    [SerializeField] float bulletSpeed = 5.0f;
    [SerializeField] bool setInactiveOnFire = true;

    [Header("References")]
    [SerializeField] GameObject bulletPrefab = null;
    public Transform tipOfGun = null;
    [SerializeField] Animator animator = null;
    [SerializeField] Transform weaponManagerTransform = null;

    List<Transform> _bulletEmitterPoints = new List<Transform>();
    List<GameObject> _bulletPool = new List<GameObject>();

    //used in aim calculation
    List<Vector3> _bulletEndPoints = new List<Vector3>();

#if UNITY_EDITOR
    [SerializeField] bool debugBulletPath = true;
#endif

    int _bulletKey = 0;
    int _poolSize = 2;
    float _lineDistance = 90.0f;


    private void Start()
    {
        GetEmitters();
        for (int i = 0; i < _poolSize; i++)
        {
            _bulletPool.Add(GameObject.Instantiate(bulletPrefab));
            _bulletPool[_bulletPool.Count - 1].SetActive(false);
        }
    }
    private void Update()
    {
        SetBulletEndPoints();
    }
    public override void Attack()
    {
        animator.SetBool("Shoot", CanAttack());
        if (!CanAttack())
            return;

        ResetCooldown();
        OnAttack.Invoke();

        foreach (var bep in _bulletEmitterPoints)
        {
            GameObject bullet = _bulletPool[_bulletKey];

            bullet.SetActive(true);
            bullet.GetComponent<Lifetime>().ResetLife();
            bullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
            bullet.transform.rotation = bep.transform.rotation;
            bullet.transform.position = bep.transform.position;
            bullet.GetComponentInChildren<TrailRenderer>().Clear();
            //bullet.transform.position = bullet.transform.position + bep.transform.TransformVector(Vector3.forward);

            var dir = bep.transform.position - tipOfGun.position;
            dir = dir.normalized;
            bullet.GetComponent<Rigidbody>().AddForce(dir * bulletSpeed, ForceMode.Impulse);
            _bulletKey = (_bulletKey + 1) % _bulletPool.Count;
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debugBulletPath)
            return;


        GetEmitters();

        for (int i = 0; i < _bulletEmitterPoints.Count; i++)
        {
            if (_bulletEmitterPoints[i] == null)
            {
                _bulletEmitterPoints.RemoveAt(i);
                _bulletEndPoints.RemoveAt(i);
                i--;
            }
        }

        Handles.color = Color.red;
        int j = 0;
        foreach (var bep in _bulletEmitterPoints)
        {
            var dir = bep.position - tipOfGun.position;
            dir = dir.normalized;
            var p2 = bep.position + dir * _lineDistance;
            Handles.DrawLine(tipOfGun.position, bep.position);
            Handles.DrawDottedLine(tipOfGun.position, p2, 20.0f);

            if (weaponManagerTransform != null)
            {
                var intersection = LineIntersection(tipOfGun.position, p2, weaponManagerTransform.position, weaponManagerTransform.position + weaponManagerTransform.forward * _lineDistance);
                if (intersection.magnitude != 0.0f)
                    Gizmos.DrawWireSphere(intersection, 1.0f);
            }
            j++;
        }
        Handles.color = Color.blue;
        if (weaponManagerTransform != null)
            Handles.DrawDottedLine(weaponManagerTransform.position, weaponManagerTransform.position + weaponManagerTransform.forward * _lineDistance, 20.0f);
    }
#endif

    void GetEmitters()
    {
        var children = tipOfGun.GetComponentInChildren<Transform>();
        foreach (Transform child in children)
        {

            if (!_bulletEmitterPoints.Contains(child))
            {
                _bulletEmitterPoints.Add(child);
                _bulletEndPoints.Add(new Vector3(0.0f, 0.0f, 0.0f));
            }

        }
        _poolSize = _bulletEmitterPoints.Count * 2;

    }

    void SetBulletEndPoints()
    {
        //Handles.color = Color.red;
        int j = 0;
        foreach (var bep in _bulletEmitterPoints)
        {
            var dir = bep.position - tipOfGun.position;
            dir = dir.normalized;
            var p2 = bep.position + dir * _lineDistance;

            var intersection = LineIntersection(tipOfGun.position, p2, weaponManagerTransform.position, weaponManagerTransform.position + weaponManagerTransform.forward * _lineDistance);
            if (intersection.magnitude != 0.0f)
            {
                _bulletEndPoints[j] = intersection;
            }
            j++;
        }
    }



    //https://web.archive.org/web/20060911055655/http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/
    Vector3 LineIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        float xzDenominator = (p4.z - p3.z) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.z - p1.z);

        if (xzDenominator == 0.0f)
            return Vector3.zero;

        float xzNumerator = (p4.x - p3.x) * (p1.z - p3.z) - (p4.z - p3.z) * (p1.x - p3.x);

        float xz = xzNumerator / xzDenominator;

        Vector3 output = Vector3.zero;
        output.x = p1.x + xz * (p2.x - p1.x);
        output.y = p1.y + xz * (p2.y - p1.y);
        output.z = p1.z + xz * (p2.z - p1.z);

        var behindTest = weaponManagerTransform.InverseTransformPoint(output);
        if (behindTest.z < 0.0f || behindTest.z >= weaponManagerTransform.InverseTransformPoint(p2).z)
            return Vector3.zero;
        return output;

    }


    public Vector3 GetBulletEndPoint(int index)
    {
        return _bulletEndPoints[index];
    }

    public void DisableAllBullets()
    {
        foreach (var b in _bulletPool)
            b.gameObject.SetActive(false);
    }
}
