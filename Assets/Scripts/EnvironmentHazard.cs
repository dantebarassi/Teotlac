using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EnvironmentHazard : MonoBehaviour
{
    [SerializeField] protected float _damage, _damageInterval, _despawnTime;

    protected bool canDamage = true;

    VisualEffect _vfx = null;

    private void Start()
    {
        if (_despawnTime > 0)
        {
            StartCoroutine(Despawn());

            if (TryGetComponent(out VisualEffect vfx)) _vfx = vfx;
        }
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

        GetComponent<Collider>().enabled = false;

        if (_vfx != null) _vfx.Stop();

        yield return new WaitForSeconds(6);

        Destroy(gameObject);
    }
}
