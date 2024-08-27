using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{
    [SerializeField] protected float _damage, _damageInterval, _despawnTime;

    protected bool canDamage = true;

    private void Start()
    {
        if (_despawnTime > 0) StartCoroutine(Despawn());
    }

    private void OnTriggerStay(Collider other)
    {
        if (!canDamage) return;

        if (other.TryGetComponent(out Entity entity))
        {
            entity.TakeDamage(_damage, true);
            StartCoroutine(DamageCooldown());
        }
    }

    IEnumerator DamageCooldown()
    {
        canDamage = false;

        yield return new WaitForSeconds(_damageInterval);

        canDamage = true;
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(_despawnTime);

        Destroy(gameObject);
    }
}
