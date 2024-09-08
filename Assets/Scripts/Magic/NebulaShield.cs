using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NebulaShield : MonoBehaviour, IDamageable
{
    [SerializeField] SunMagic _overchargeProjectile;
    [SerializeField] float _duration, _durationPerBlock, _contactDmg, _overchargeDmg, _overchargeSpeed;
    [SerializeField] int _overchargeThreshold;
    [HideInInspector] public Boss target;
    int _blockCounter = 0;

    private void Update()
    {
        _duration -= Time.deltaTime;
        
        if (_duration <= 0)
        {
            Expire();
        }
    }

    void Expire()
    {
        Destroy(gameObject);
    }

    void Overcharge()
    {
        GetComponent<Collider>().enabled = false;

        var projectile = Instantiate(_overchargeProjectile, transform.position, transform.rotation);
        projectile.transform.localScale *= 8;
        projectile.SetupStats(_overchargeDmg);

        if (target != null) projectile.transform.forward = (target.transform.position + Vector3.up * 1.5f) - transform.position;
        else projectile.transform.forward = transform.up;

        projectile.Shoot(_overchargeSpeed);

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == target.gameObject)
        {
            target.TakeDamage(_contactDmg);
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
            transform.localScale += Vector3.one * 0.1f;
            _duration += _durationPerBlock;
        }
    }
    
    public void Die()
    {
        Destroy(gameObject);
    }
}
