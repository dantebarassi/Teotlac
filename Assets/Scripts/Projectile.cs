using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IDamageable
{
    [HideInInspector] public float damage, speed;

    [SerializeField] protected float _deathTimer;

    protected virtual void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        if (_deathTimer <= 0)
        {
            Die();
        }
        else
        {
            _deathTimer -= Time.deltaTime;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 11)
        {
            return;
        }

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
        }

        Die();
    }

    public virtual void TakeDamage(float amount, bool bypassCooldown = false)
    {
        Die();
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
