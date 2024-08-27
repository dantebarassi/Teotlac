using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunBasic : PlayerProjectile
{
    [SerializeField] Rigidbody _rb;
    [SerializeField] EnvironmentHazard _sunFire;

    bool _shot = false;

    protected override void Update()
    {
        if (_deathTimer <= 0)
        {
            Die();
        }
        else if (_shot)
        {
            _deathTimer -= Time.deltaTime;
        }
    }

    public void Launch(Vector3 dir, float strength)
    {
        _shot = true;
        _rb.isKinematic = false;
        _rb.AddForce(dir * strength);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6 || other.gameObject.layer == 7) return;

        if (other.gameObject.layer == 10)
        {
            Instantiate(_sunFire, new Vector3(transform.position.x, other.transform.position.y, transform.position.z), Quaternion.identity);
        }
        else if (other.TryGetComponent(out IDamageable damageable))
        {
            Debug.Log("Hago daño");
            damageable.TakeDamage(_damage);
        }
        else
        {
            var parent = other.GetComponentInParent<IDamageable>();

            if (parent != null)
            {
                parent.TakeDamage(_damage);
            }
        }

        Die();
    }
}
