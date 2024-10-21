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
    //[SerializeField] Rigidbody _rb;
    [SerializeField] LayerMask _playerLayer;
    [SerializeField] float _height, _radius, _riseDuration, _riseDamageDelay, _bloomHorSpawnVariation, _bloomVerSpawnVariation, _bloomHorDirVariation, _bloomVerDirVariation;
    float _strikeDamage, _shardSpeed, _shardDamage;
    bool _rising = false;

    public void Initialize(ObjectPool<ObsidianBud> budPool, ObjectPool<ObsidianShard> shardPool, Action<ObsidianBud> onDestroy, Vector3 spawnPos, float strikeDmg, float projectileSpeed, float projectileDamage)
    {
        _budPool = budPool;
        _shardPool = shardPool;
        _onDestroy = onDestroy;
        _strikeDamage = strikeDmg;
        _shardSpeed = projectileSpeed;
        _shardDamage = projectileDamage;

        StartCoroutine(Rising(spawnPos));
    }

    IEnumerator Rising(Vector3 endPos)
    {
        _rising = true;
        //_rb.isKinematic = true;

        float timer = 0;
        bool hit = false;
        Vector3 startPos = endPos - new Vector3(0, _height);

        while (timer < _riseDuration)
        {
            timer += Time.deltaTime;

            transform.position = Vector3.Lerp(startPos, endPos, timer / _riseDuration);

            if (!hit && timer >= _riseDamageDelay)
            {
                if (Physics.CheckCapsule(transform.position, transform.position + Vector3.up * _height, _radius, _playerLayer))
                {
                    hit = true;

                    GameManager.instance.player.TakeDamage(_strikeDamage);
                }
            }

            yield return null;
        }

        _rising = false;
        //_rb.isKinematic = false;
    }

    /*private void FixedUpdate()
    {
        if (!_rising) return;

        Debug.Log(_startpos + " - " + _endPos);

        if (_timer < _riseDuration)
        {
            _timer += Time.fixedDeltaTime;

            _rb.MovePosition(Vector3.Lerp(_startpos, _endPos, _timer / _riseDuration));
        }
        else
        {
            _rising = false;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }*/

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
            shard.transform.forward = dir.VectorVariation(i, _bloomHorDirVariation, _bloomVerDirVariation);
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
        _onDestroy(this);

        Die();
    }

    public void Die()
    {
        // particulas y bla

        _bud.SetActive(true);
        _preBloom.SetActive(false);

        _budPool.RefillStock(this);
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (!_rising) return;

        if (collision.gameObject.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage(_strikeDamage);
        }
    }*/
}