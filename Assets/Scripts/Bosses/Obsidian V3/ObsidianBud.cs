using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObsidianBud : MonoBehaviour, IDamageable
{
    ObjectPool<ObsidianBud> _budPool;
    ObjectPool<ObsidianShard> _shardPool;
    Action<ObsidianBud> _onDestroy;
    [SerializeField] GameObject _bud, _preBloom;
    [SerializeField] float _bloomHorSpawnVariation, _bloomVerSpawnVariation, _bloomHorDirVariation, _bloomVerDirVariation;
    float _shardSpeed, _shardDamage;

    public void Initialize(ObjectPool<ObsidianBud> budPool, ObjectPool<ObsidianShard> shardPool, Action<ObsidianBud> onDestroy, float projectileSpeed, float projectileDamage)
    {
        _budPool = budPool;
        _shardPool = shardPool;
        _onDestroy = onDestroy;
        _shardSpeed = projectileSpeed;
        _shardDamage = projectileDamage;
    }

    public void Prebloom()
    {
        // prender particulas y bla
        _bud.SetActive(false);
        _preBloom.SetActive(true);
    }

    public void Bloom(Vector3 targetPos, int projectileCount)
    {
        // prender particulas y bla
        var dir = targetPos - transform.position;

        for (int i = 0; i < projectileCount; i++)
        {
            var shard = _shardPool.Get();
            shard.Initialize(_shardPool, _shardSpeed, _shardDamage);
            shard.transform.position = transform.position.VectorVariation(1, _bloomHorSpawnVariation, _bloomVerSpawnVariation, _bloomHorSpawnVariation);
            shard.transform.forward = dir.VectorVariation(i * 0.5f, _bloomHorDirVariation, _bloomVerDirVariation);
        }

        Die();
    }

    public static void TurnOff(ObsidianBud x)
    {
        x.gameObject.SetActive(false);
    }

    public static void TurnOn(ObsidianBud x)
    {
        x.gameObject.SetActive(true);
    }

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        Die();
    }

    public void Die()
    {
        // particulas y bla

        _onDestroy(this);

        _budPool.RefillStock(this);
    }
}
