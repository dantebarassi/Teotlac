using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObsidianShard : Projectile
{
    ObjectPool<ObsidianShard> _objectPool;
    Action<Vector3> _spawnBud;

    [SerializeField] LayerMask _groundLayer;
    [SerializeField] ObsidianBud _bud;
    [Range(0, 100)]
    [SerializeField] int _budSpawnChance;

    public void Initialize(ObjectPool<ObsidianShard> pool, float spd, float dmg,Action<Vector3> spawnBud = null)
    {
        _objectPool = pool;
        speed = spd;
        damage = dmg;
        _spawnBud = spawnBud;
    }

    public static void TurnOff(ObsidianShard x)
    {
        x.gameObject.SetActive(false);
    }

    public static void TurnOn(ObsidianShard x)
    {
        x._timer = x._duration; 

        x.gameObject.SetActive(true);
    }

    public override void Die()
    {
        _objectPool.RefillStock(this);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 7 || other.gameObject.layer == 11) return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
        }
        else if (other.gameObject.layer == 10)
        {
            if (_spawnBud != null && UnityEngine.Random.Range(0, 100) < _budSpawnChance)
            {
                if (Physics.Raycast(transform.position, Vector3.down, out var hit, 1, _groundLayer)) _spawnBud(hit.point);
            }
        }

        Die();
    }
}
