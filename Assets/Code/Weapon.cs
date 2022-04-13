using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    [Header("References")]
    public GameObject weaponOwner = null;

    [Tooltip("-1 for infinite")]
    [SerializeField] protected float cooldown = 0.0f;
    [SerializeField] protected float finishedAttackCooldown = 0.5f;

    [Tooltip("Adds another check for CanAttack() to pass or fail")]
    [SerializeField] protected bool _canAttackOverride = true;

    public UnityEvent OnAttack;
    public UnityEvent OnFinishAttack;
    public UnityEvent OnEnableWeapon;
    public UnityEvent OnDisableWeapon;

    bool _bFinishAttack = true;
    float _finishAttack = 0.0f;
    float _cooldown = 0.0f;
    protected bool _attacking = false;

    private void OnEnable()
    {
        _cooldown = cooldown;
        _finishAttack = 0.0f;
        OnEnableWeapon.Invoke();
        IEnumerator Cooldown()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                _finishAttack += Time.deltaTime;
                if (_finishAttack >= finishedAttackCooldown && !_bFinishAttack)
                {
                    _bFinishAttack = true;
                    OnFinishAttack.Invoke();
                }
                _cooldown -= Time.deltaTime;
            }
        }
        StartCoroutine(Cooldown());
    }

    private void OnDisable()
    {
        OnDisableWeapon.Invoke();
    }

    protected float GetCooldown()
    {
        return _cooldown;
    }

    protected bool IsAttacking()
    {
        return _attacking;
    }

    public bool CanAttack()
    {
        return (!_attacking && _cooldown < 0.0f && _canAttackOverride);
    }

    public void SetCanAttackOverride(bool yn)
    {
        _canAttackOverride = yn;
    }

    public void SetAttacking(bool b)
    {
        _attacking = b;
    }

    protected void ResetCooldown()
    {
        _cooldown = cooldown;
        _finishAttack = 0.0f;
        _bFinishAttack = false;
    }

    public virtual void Attack()
    {
        if (!CanAttack())
            return;
        ResetCooldown();
        OnAttack.Invoke();
    }

}
