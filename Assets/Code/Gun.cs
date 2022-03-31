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

    List<Transform> _bulletEmitterPoints = new List<Transform>();
    List<GameObject> _bulletPool = new List<GameObject>();

#if UNITY_EDITOR
    [SerializeField] bool debugBulletPath = true;
#endif

    int _bulletKey = 0;
    int _poolSize = 2;

    private void Start()
    {
        GetEmitters();
        for (int i = 0; i < _poolSize; i++)
        {
            _bulletPool.Add(GameObject.Instantiate(bulletPrefab));
            _bulletPool[_bulletPool.Count - 1].SetActive(false);
        }
    }
    public override void Attack()
    {
        animator.SetBool("Shoot",CanAttack());
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
                i--;
            }
        }


        Handles.color = Color.red;
        foreach (var bep in _bulletEmitterPoints)
        {
            var dir = bep.position - tipOfGun.position;
            dir = dir.normalized;
            Handles.DrawLine(tipOfGun.position, bep.position);
            Handles.DrawDottedLine(tipOfGun.position, bep.position + dir * 25.0f, 10.0f);
        }
        Handles.color = Color.blue;
        Handles.DrawDottedLine(transform.parent.position, transform.parent.position + transform.parent.forward * 25.0f, 10.0f);
    }
#endif

    void GetEmitters()
    {
        var children = tipOfGun.GetComponentInChildren<Transform>();
        foreach (Transform child in children)
            if (!_bulletEmitterPoints.Contains(child))
                _bulletEmitterPoints.Add(child);
        _poolSize = _bulletEmitterPoints.Count * 2;

    }
}
