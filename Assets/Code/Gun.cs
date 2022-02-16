using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] int poolSize = 2;
    [SerializeField] float bulletSpeed = 5.0f;
    [SerializeField] bool setInactiveOnFire = true;

    [Header("References")]
    [SerializeField] GameObject bulletPrefab = null;
    [SerializeField] List<Transform> bulletEmitterPoints = new List<Transform>();
    [SerializeField] Animator animator = null;

    List<GameObject> _bulletPool = new List<GameObject>();
    int _bulletKey = 0;
    private void Start()
    {

        for (int i = 0; i < poolSize; i++)
        {
            _bulletPool.Add(GameObject.Instantiate(bulletPrefab));
            _bulletPool[_bulletPool.Count - 1].SetActive(false);
        }
    }
    public override void Attack()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            return;
        ResetCooldown();
        OnAttack.Invoke();
        animator.SetTrigger("Shoot");
        foreach (var bep in bulletEmitterPoints)
        {
            GameObject bullet = _bulletPool[_bulletKey];

            bullet.SetActive(true);
            bullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
            bullet.transform.rotation = bep.transform.rotation;
            bullet.transform.position = bep.transform.position;
            bullet.transform.position = bullet.transform.position + bep.transform.TransformVector(Vector3.forward);

            var dir = bullet.transform.position - bep.transform.position;
            dir = dir.normalized;
            bullet.GetComponent<Rigidbody>().AddForce(dir * bulletSpeed, ForceMode.Impulse);
            _bulletKey = (_bulletKey + 1) % _bulletPool.Count;
        }
    }
}
