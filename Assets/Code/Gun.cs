using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] AnimationCurve gunTopLerpCurve;
    [SerializeField] AnimationCurve gunLerpCurve;
    [SerializeField] float lerpSpeed = 1.0f;
    [SerializeField] int poolSize = 2;
    [SerializeField] float bulletSpeed = 5.0f;
    [SerializeField] bool setInactiveOnFire = true;

    [Header("References")]
    [SerializeField] GameObject bulletPrefab = null;
    [SerializeField] Transform gunTop;
    [SerializeField] Transform gun;
    [SerializeField] Transform gunTopLerpPoint;
    [SerializeField] Transform gunLerpPoint;
    [SerializeField] List<Transform> bulletEmitterPoints = new List<Transform>();

    private Vector3 _orignalPositionGunTop;

    private Vector3 _orignalPositionGun;
    private Quaternion _orignalPositionRotationGun;


    List<GameObject> _bulletPool = new List<GameObject>();
    private void Start()
    {
        _orignalPositionGunTop = gunTop.transform.localPosition;
        _orignalPositionGun = gun.transform.localPosition;
        _orignalPositionRotationGun = gun.transform.localRotation;

        for (int i = 0; i < poolSize; i++)
        {
            _bulletPool.Add(GameObject.Instantiate(bulletPrefab));
            _bulletPool[_bulletPool.Count - 1].SetActive(false);
        }
    }
    public override void Attack()
    {
        IEnumerator Lerp()
        {
            _attacking = true;
            float x = 0.0f;
            while (x < 1.0f)
            {
                yield return new WaitForEndOfFrame();
                x += Time.deltaTime * lerpSpeed;
                gunTop.transform.localPosition = Vector3.Lerp(_orignalPositionGunTop, gunTopLerpPoint.localPosition, gunTopLerpCurve.Evaluate(x));
                gun.transform.localPosition = Vector3.Slerp(_orignalPositionGun, gunLerpPoint.localPosition, gunLerpCurve.Evaluate(x));
                gun.transform.localRotation = Quaternion.Slerp(_orignalPositionRotationGun, gunLerpPoint.localRotation, gunLerpCurve.Evaluate(x));
            }
            x = 1.0f;
            while (x > 0.0f)
            {
                yield return new WaitForEndOfFrame();
                x -= Time.deltaTime * lerpSpeed;
                gunTop.transform.localPosition = Vector3.Lerp(_orignalPositionGunTop, gunTopLerpPoint.localPosition, gunTopLerpCurve.Evaluate(x));
                gun.transform.localPosition = Vector3.Slerp(_orignalPositionGun, gunLerpPoint.localPosition, gunLerpCurve.Evaluate(x));
                gun.transform.localRotation = Quaternion.Slerp(_orignalPositionRotationGun, gunLerpPoint.localRotation, gunLerpCurve.Evaluate(x));
            }
            x = 0.0f;
            _attacking = false;
            gameObject.SetActive(!setInactiveOnFire);
        }
        StartCoroutine(Lerp());
        foreach (var bep in bulletEmitterPoints)
        {
            GameObject bullet = null;
            foreach (var b in _bulletPool)
                if (!b.activeSelf)
                    bullet = b;
            if (bullet == null)
                bullet = _bulletPool[_bulletPool.Count - 1];

            bullet.SetActive(true);
            bullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
            bullet.transform.rotation = bep.transform.rotation;
            bullet.transform.position = bep.transform.position;
            bullet.transform.position = bullet.transform.position + bep.transform.TransformVector(Vector3.forward);

            var dir = bullet.transform.position - bep.transform.position;
            dir = dir.normalized;
            bullet.GetComponent<Rigidbody>().AddForce(dir * bulletSpeed, ForceMode.Impulse);
        }
        base.Attack();
    }
}
