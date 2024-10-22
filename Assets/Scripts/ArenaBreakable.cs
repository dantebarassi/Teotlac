using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaBreakable : MonoBehaviour, IDamageable
{
    [SerializeField] float _hp;

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        _hp -= amount;

        if (_hp <= 0) Die();
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
