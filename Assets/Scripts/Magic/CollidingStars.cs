using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CollidingStars : MonoBehaviour
{
    [SerializeField] GameObject _sunPositive, _sunNegative;
    [SerializeField] Transform _axis;
    [SerializeField] VisualEffect _explosion;
    [SerializeField] float _moveSpeed, _rotationSpeed, _explosionRotationSpeed, _contactDamage, _explosionDamage, _explosionRadius, _explosionDelay, _explosionDuration, _expirationTime;
    [SerializeField] LayerMask _explosionTargets;
    [SerializeField] Rigidbody _rb;

    [HideInInspector] public PlayerController player;

    bool _dead;

    void Update()
    {
        _axis.rotation = Quaternion.AngleAxis(_rotationSpeed * Time.deltaTime, Vector3.up) * _axis.rotation;
        
        if (!_dead)
        {
            if (_expirationTime <= 0)
            {
                StartCoroutine(Expire());
            }
            else
            {
                _expirationTime -= Time.deltaTime;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!_dead)
        {
            _rb.MovePosition(transform.position + transform.forward * _moveSpeed * Time.fixedDeltaTime);
        }
    }

    public void Collide()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Expire()
    {
        _dead = true;

        player.Specials.ActivateSpecial(SpecialsManager.Specials.StarCollision, true);
        // stop o apagar vfx principal o prender algun otro para cuando expire o choque

        yield return new WaitForSeconds(2); // esperar lo que tardaria en desaparecer

        Destroy(gameObject);
    }

    IEnumerator Explosion()
    {
        _dead = true;

        float timer = 0, baseX = _sunPositive.transform.localPosition.x, baseRotationSpeed = _rotationSpeed, lerpT;

        while (timer < _explosionDelay)
        {
            timer += Time.deltaTime;

            lerpT = timer / _explosionDelay;

            _sunPositive.transform.localPosition = new Vector3(Mathf.Lerp(baseX, 0, lerpT), 0);
            _sunNegative.transform.localPosition = new Vector3(Mathf.Lerp(-baseX, 0, lerpT), 0);
            _rotationSpeed = Mathf.Lerp(baseRotationSpeed, _explosionRotationSpeed, lerpT);

            yield return null;
        }

        _sunPositive.SetActive(false);
        _sunNegative.SetActive(false);
        _explosion.gameObject.SetActive(true);
        
        timer = 0;
        List<Collider> ignore = new List<Collider>();
        bool skip = false;

        while (timer < _explosionDuration)
        {
            var cols = Physics.OverlapSphere(transform.position, _explosionRadius, _explosionTargets);

            foreach (var item in cols)
            {
                foreach (var item2 in ignore)
                {
                    if (item == item2) skip = true;
                }

                if (skip)
                {
                    skip = false;
                    continue;
                }

                if (item.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(_explosionDamage);

                    if (item != null) ignore.Add(item);
                }
                else
                {
                    damageable = item.GetComponentInParent<IDamageable>();

                    if (damageable != null) damageable.TakeDamage(_explosionDamage);

                    if (item != null) ignore.Add(item);
                }
            }

            timer += Time.deltaTime;

            yield return null;
        }

        //_explosion.Stop();

        yield return new WaitForSeconds(5); // esperar lo que tardaria en desaparecer la explosion

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_dead) return;

        if (other.gameObject.layer == 6 || other.gameObject.layer == 11 || other.gameObject.layer == 13)
        {
            return;
        }

        if (other.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(_contactDamage);
        }
        else
        {
            var parent = other.GetComponentInParent<IDamageable>();

            if (parent != null)
            {
                parent.TakeDamage(_contactDamage);
            }
        }

        StartCoroutine(Expire());
    }
}
