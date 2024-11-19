using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantObsidianKnife : MonoBehaviour
{
    float _thrustDamage, _sliceDamage, _currentDamage;

    Collider _col;

    void Awake()
    {
        _col = GetComponent<Collider>();
    }

    public void SetUp(float thrustDmg, float sliceDmg)
    {
        _thrustDamage = thrustDmg;
        _sliceDamage = sliceDmg;
    }

    public void Slice()
    {
        _currentDamage = _sliceDamage;

        _col.enabled = true;
    }

    public void Thrust()
    {
        _currentDamage = _thrustDamage;

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
            damageable.TakeDamage(_currentDamage);
        }
    }
}
