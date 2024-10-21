using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IDamageable
{
    [HideInInspector] public float damage, speed;

    [SerializeField] protected float _duration;
    [SerializeField] protected Rigidbody _rb;
    protected float _timer;

    protected void Start()
    {
        _timer = _duration;
    }

    protected virtual void Update()
    {
        if (_timer <= 0)
        {
            Die();
        }
        else
        {
            _timer -= Time.deltaTime;
        }
    }

    protected void FixedUpdate()
    {
        _rb.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 11) return;

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
