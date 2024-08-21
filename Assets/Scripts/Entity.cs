using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected float _maxHp;

    protected float _hp;

    protected Rigidbody _rb;

    protected virtual void Awake()
    {
        _hp = _maxHp;
        _rb = GetComponent<Rigidbody>();
    }

    public virtual void KnockBack(Vector3 dir, float strength)
    {
        _rb.velocity = Vector3.zero;
        _rb.AddForce(dir * strength);
    }

    public virtual void TakeDamage(float amount, bool bypassCooldown = false)
    {
        _hp -= amount;

        if (_hp <= 0)
        {
            Die();
        }
    }

    public abstract void Die();
}
