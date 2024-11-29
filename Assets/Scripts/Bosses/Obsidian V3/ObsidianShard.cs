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
    [SerializeField] float _homingStrength;

    [SerializeField] AudioSource _audioSource;

    bool _shot = false;

    Transform _target;

    public void Initialize(ObjectPool<ObsidianShard> pool, float spd, float dmg, Transform target = null, Action<Vector3> spawnBud = null)
    {
        _objectPool = pool;
        speed = spd;
        damage = dmg;
        _target = target;
        _spawnBud = spawnBud;
        _shot = true;
    }

    protected override void Update()
    {
        if (!_shot) return;

        base.Update();
    }

    protected override void FixedUpdate()
    {
        if (!_shot) return;

        Vector3 dir;
        
        if (_target != null)
        {
            Vector3 desiredDir = (_target.position - transform.position).normalized;
        
            dir = Vector3.Lerp(transform.forward, desiredDir, _homingStrength * Time.fixedDeltaTime / Vector3.Angle(transform.forward, desiredDir)).normalized;

            _rb.MoveRotation(Quaternion.LookRotation(dir));
        }
        else
        {
            dir = transform.forward;
        }
        
        _rb.MovePosition(transform.position + speed * Time.fixedDeltaTime * dir);
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
        StartCoroutine(Death());
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!_shot || other.gameObject.layer == 3 || other.gameObject.layer == 7 || other.gameObject.layer == 11) return;

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

    IEnumerator Death()
    {
        //_audioSource.PlayOneShot(AudioManager.instance.shardHit);

        yield return new WaitForSeconds(2);

        _objectPool.RefillStock(this);
    }
}
