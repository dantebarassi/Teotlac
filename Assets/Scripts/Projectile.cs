using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IDamageable
{
    [HideInInspector] public float damage, speed;

    [SerializeField] float _deathTimer;

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        if (_deathTimer <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            _deathTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 7 || other.gameObject.layer == 11)
        {
            return;
        }

        if (other.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage(damage);
        }

        Die();
    }

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        Die();
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
