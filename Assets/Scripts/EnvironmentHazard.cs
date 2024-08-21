using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{
    [SerializeField] float _damage, _damageInterval;

    bool canDamage = true;

    private void OnTriggerStay(Collider other)
    {
        if (!canDamage) return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage, true);
            StartCoroutine(DamageCooldown());
        }
    }

    IEnumerator DamageCooldown()
    {
        canDamage = false;

        yield return new WaitForSeconds(_damageInterval);

        canDamage = true;
    }
}
