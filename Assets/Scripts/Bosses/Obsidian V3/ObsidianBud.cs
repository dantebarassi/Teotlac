using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsidianBud : MonoBehaviour, IDamageable
{
    ObjectPool<ObsidianBud> _objectPool;
    [SerializeField] GameObject _bud, _preBloom;
    [SerializeField] Projectile _projectile;
    [SerializeField] float _bloomHorizontalSpawnVariation, _bloomVerticalSpawnVariation, _bloomHorizontalDirVariation, _bloomVerticalDirVariation;
    float _projectileSpeed, _projectileDamage;

    public void Initialize(ObjectPool<ObsidianBud> op, float projectileSpeed, float projectileDamage)
    {
        _objectPool = op;
        _projectileSpeed = projectileSpeed;
        _projectileDamage = projectileDamage;
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
            var shard = Instantiate(_projectile, transform.position.VectorVariation(1, _bloomHorizontalSpawnVariation, _bloomVerticalSpawnVariation, _bloomHorizontalSpawnVariation), Quaternion.identity);
            shard.transform.forward = dir.VectorVariation(i * 0.5f, _bloomHorizontalDirVariation, _bloomVerticalDirVariation);
            shard.speed = _projectileSpeed;
            shard.damage = _projectileDamage;
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

        _objectPool.RefillStock(this);
    }
}
