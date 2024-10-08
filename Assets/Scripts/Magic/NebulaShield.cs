using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class NebulaShield : MonoBehaviour, IDamageable
{
    [SerializeField] SunMagic _overchargeProjectile;
    [SerializeField] float _duration, _durationPerBlock, _contactDmg, _overchargeMinDmg, _overchargeMaxDmg, _overchargeSpeed, _growRate, _growDuration, _invertDuration, _lingerDuration;
    [SerializeField] int _overchargeThreshold;
    Boss _target;
    PlayerController _player;
    int _blockCounter = 0, _growQueue = 0;
    bool _growing = false;
    [SerializeField] VisualEffect _explotion,_nebulosa,_galaxy;

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
        StartCoroutine(Overcharging());
    }

    IEnumerator Overcharging()
    {
        // la locura que se invierte y concentra <3
        _nebulosa.SetFloat("Explotion", 1);
        _galaxy.SetFloat("RotationVelocity", -13);
        _galaxy.SetFloat("Explotion", 3);
        yield return new WaitForSeconds(_invertDuration);
        _nebulosa.gameObject.SetActive(false);
        ShootProjectile();
        yield return new WaitForSeconds(_lingerDuration);

        Destroy(gameObject);
    }

    public void ShootProjectile()
    {
        GetComponent<Collider>().enabled = false;
        _explotion.gameObject.SetActive(true);
        var projectile = Instantiate(_overchargeProjectile, transform.position, transform.rotation);
        projectile.SetupStats(Mathf.Lerp(_overchargeMinDmg, _overchargeMaxDmg, _blockCounter / _overchargeThreshold));

        if (_target != null) projectile.transform.forward = (_target.transform.position + transform.forward * 1.5f) - transform.position;
        else projectile.transform.forward = transform.forward;

        projectile.Shoot(_overchargeSpeed);

        _player.Specials.ActivateSpecial(SpecialsManager.Specials.NebulaShield, true);
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
        //_explotion.gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
