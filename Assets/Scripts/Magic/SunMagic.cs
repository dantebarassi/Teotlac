using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMagic : PlayerProjectile
{
    ObjectPool<SunMagic> _objectPool;

    [SerializeField] ParticleSystem _chargingParticles, _readyParticles, _destroyedParticles, _baseParticle, _fireParticle, _trailParticle, _destroyedSparksParticles;

    [HideInInspector] public PlayerController player;

    [SerializeField] Light _light;

    [SerializeField] float _lightRangeMultiplier, _growRate;

    [SerializeField] Rigidbody _rb;
    [SerializeField] AudioSource _audioSource;

    bool _charging = true, _shot = false, _dead = false;

    public void Initialize(ObjectPool<SunMagic> op, float dmg, float speed)
    {
        _objectPool = op;
        SetupStats(dmg);
        Shoot(speed);
    }

    public void UpdateDamage(float add)
    {
        _damage += add;
    }

    protected override void Update()
    {
        if (_charging)
        {
            //_baseParticle.gameObject.transform.localScale += Vector3.one * Time.deltaTime * _growRate;
            //_fireParticle.gameObject.transform.localScale += Vector3.one * Time.deltaTime * _growRate;
            _light.range += Time.deltaTime * _lightRangeMultiplier;
            
        }
        else if (_shot && !_dead)
        {
            if (_deathTimer <= 0)
            {
                StartCoroutine(Death());
            }
            else
            {
                _deathTimer -= Time.deltaTime;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_shot && !_dead)
        {
            _rb.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!_shot || _dead) return;
        
        if (other.gameObject.layer == 6 || other.gameObject.layer == 11 || other.gameObject.layer == 13)
        {
            return;
        }

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_damage);
        }
        else
        {
            var parent = other.GetComponentInParent<IDamageable>();

            if (parent != null)
            {
                parent.TakeDamage(_damage);
            }
        }

        StartCoroutine(Death());
    }

    public void ChargeFinished()
    {
        _chargingParticles.Stop();
        _readyParticles.Play();
        _charging = false;
    }

    public void Shoot(float speed)
    {
        _speed = speed;
        _chargingParticles.Stop();
        _fireParticle.Stop();
        _trailParticle.Play();
        _charging = false;
        _shot = true;
    }

    public void Absorb(float absorbTime)
    {
        _chargingParticles.Stop();
        _charging = false;
        StartCoroutine(Shrink(absorbTime));
    }

    IEnumerator Shrink(float duration)
    {
        float timer = 0;

        var startingScale = transform.localScale;

        while (timer <= duration)
        {
            timer += Time.deltaTime;

            transform.localScale = Vector3.Lerp(startingScale, Vector3.zero, timer / duration);

            yield return null;
        }

        Die();
    }

    public IEnumerator Death()
    {
        _dead = true;
        _destroyedParticles.Play();
        _destroyedSparksParticles.Play();
        GetComponentInChildren<Renderer>().enabled = false;

        _audioSource.PlayOneShot(AudioManager.instance.ComboHit());

        yield return new WaitForSeconds(2.5f);

        if (_objectPool != null) _objectPool.RefillStock(this);
        else Destroy(gameObject);
    }

    public IEnumerator Cancel()
    {
        _dead = true;
        //play particles
        GetComponentInChildren<Renderer>().enabled = false;

        yield return new WaitForSeconds(1);

        Die();
    }

    public static void TurnOff(SunMagic x)
    {
        x.gameObject.SetActive(false);
    }

    public static void TurnOn(SunMagic x)
    {
        x.gameObject.SetActive(true);

        x._dead = false;
    }
}
