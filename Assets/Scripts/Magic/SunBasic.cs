using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SunBasic : PlayerProjectile
{
    [SerializeField] Rigidbody _rb;
    [SerializeField] EnvironmentHazard _sunFire;
    [SerializeField] VisualEffect _sun, _nova;
    [SerializeField] Light _light;

    bool _shot = false, _dead = false;
    float _timer = 0, _baseTemp;

    private void Start()
    {
        _baseTemp = _light.colorTemperature;
    }

    protected override void Update()
    {
        if (_deathTimer <= 0)
        {
            StartCoroutine(Death());
        }
        else if (_shot && !_dead)
        {
            _deathTimer -= Time.deltaTime;
        }
        else
        {
            _timer += Time.deltaTime;

            if (_timer >= 4)
            {
                _light.colorTemperature = Mathf.Lerp(_baseTemp, 20000, (_timer - 4) / 2.5f);
            }
        }
    }

    public void Launch(Vector3 dir, float strength)
    {
        _shot = true;
        _rb.isKinematic = false;
        _rb.AddForce(dir * strength);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6 || other.gameObject.layer == 7 || _dead || !_shot) return;

        if (other.gameObject.layer == 10)
        {
            Instantiate(_sunFire, new Vector3(transform.position.x, other.transform.position.y, transform.position.z), Quaternion.identity);
        }
        else if (other.TryGetComponent(out IDamageable damageable))
        {
            Debug.Log("Hago daño");
            damageable.TakeDamage(_damage);
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

    public IEnumerator Death()
    {
        _dead = true;
        _rb.isKinematic = true;
        _sun.gameObject.SetActive(false);
        _nova.gameObject.SetActive(true);

        yield return new WaitForSeconds(4);

        _nova.Stop();
        float timer = 0;
        var baseIntensity = _light.intensity;
        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            _light.intensity = Mathf.Lerp(baseIntensity, 0, timer / 1.2f);
            yield return null;
        }

        yield return new WaitForSeconds(1);

        Die();
    }
}
