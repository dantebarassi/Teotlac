using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : PlayerProjectile
{
    [SerializeField] Rigidbody _rb;

    private void Awake()
    {
        _deathTimer = 10;
    }

    protected override void Update()
    {
        _deathTimer -= Time.deltaTime;

        if (_deathTimer <= 0)
        {
            Die();
        }
    }

    public void Toss(Vector3 dir, float strength, float damage)
    {
        _damage = damage;
        _rb.AddForce(dir.normalized * strength);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6 || other.gameObject.layer == 7)
        {
            return;
        }

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damage);
        }
        else
        {
            var parent = other.GetComponentInParent<IDamageable>();

            if (parent != null)
            {
                parent.TakeDamage(_damage);
            }
        }
    }
}
