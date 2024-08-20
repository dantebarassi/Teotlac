using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    protected float _damage, _speed, _deathTimer;

    public void SetupStats(float damage = 2, float speed = 25, float duration = 5)
    {
        _damage = damage;
        _speed = speed;
        _deathTimer = duration;
    }

    protected virtual void Update()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;

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
        if (other.gameObject.layer == 6 || other.gameObject.layer == 7)
        {
            return;
        }

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damage);
        }

        Die();
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
