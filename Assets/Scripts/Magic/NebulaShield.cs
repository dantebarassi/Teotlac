using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NebulaShield : MonoBehaviour, IDamageable
{
    [SerializeField] SunMagic _overchargeProjectile;
    [SerializeField] float _duration, _durationPerBlock, _contactDmg, _overchargeMinDmg, _overchargeMaxDmg, _overchargeSpeed, _growRate, _growDuration;
    [SerializeField] int _overchargeThreshold;
    Boss _target;
    PlayerController _player;
    int _blockCounter = 0, _growQueue = 0;
    bool _growing = false;

    private void Update()
    {
        _duration -= Time.deltaTime;
        
        if (_duration <= 0)
        {
            Expire();
        }
    }

    public void SetUp(PlayerController player)
    {
        _player = player;
        _target = player.currentBoss;
    }

    void Expire()
    {
        _player.Specials.ActivateSpecial(SpecialsManager.Specials.NebulaShield, true);

        Destroy(gameObject);
    }

    public void Supercharge()
    {
        _blockCounter = _overchargeThreshold;
        Overcharge();
    }

    public void Overcharge()
    {
        GetComponent<Collider>().enabled = false;

        var projectile = Instantiate(_overchargeProjectile, transform.position, transform.rotation);
        projectile.SetupStats(Mathf.Lerp(_overchargeMinDmg, _overchargeMaxDmg, _blockCounter / _overchargeThreshold));

        if (_target != null) projectile.transform.forward = (_target.transform.position + Vector3.up * 1.5f) - transform.position;
        else projectile.transform.forward = transform.up;

        projectile.Shoot(_overchargeSpeed);

        _player.Specials.ActivateSpecial(SpecialsManager.Specials.NebulaShield, true);

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_target == null) return;

        if (collision.gameObject == _target.gameObject)
        {
            _target.TakeDamage(_contactDmg);
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        _blockCounter++;

        if (_blockCounter >= _overchargeThreshold)
        {
            Overcharge();
        }
        else
        {
            if (!_growing) StartCoroutine(Grow());
            else _growQueue++;
            _duration += _durationPerBlock;
        }
    }

    IEnumerator Grow()
    {
        _growing = true;

        float timer = 0;

        Vector3 oldScale = transform.localScale, newScale = transform.localScale + Vector3.one * _growRate;

        while (timer < _growDuration)
        {
            timer += Time.deltaTime;

            transform.localScale = Vector3.Lerp(oldScale, newScale, timer / _growDuration);

            yield return null;
        }

        if (_growQueue > 0)
        {
            _growQueue--;
            StartCoroutine(Grow());
        }
        else
        {
            _growing = false;
        }
    }   
    
    public void Die()
    {
        Destroy(gameObject);
    }
}
