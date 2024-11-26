using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantObsidianKnife : MonoBehaviour
{
    float _damage;
    [SerializeField] Collider _col;

    public void SetUp(float damage)
    {
        _damage = damage;
    }

    public void StartHitting()
    {
        _col.enabled = true;
    }

    public void StopHitting()
    {
        _col.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage);
        }
    }
}
