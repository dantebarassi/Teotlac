using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidingStars : MonoBehaviour
{
    [SerializeField] float _speed, _contactDamage, _explosionDamage, _explosionRadius, _explosionDelay, _explosionDuration, _expirationTime;

    [SerializeField] Rigidbody _rb;

    [HideInInspector] public PlayerController player;

    bool _dead;

    void Update()
    {
        if (!_dead)
        {
            if (_expirationTime <= 0)
            {
                StartCoroutine(Death());
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
            _rb.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
        }
    }

    public void Collide()
    {
        StartCoroutine(Collision());
    }

    IEnumerator Death()
    {
        // stop o apagar vfx principal o prender algun otro para cuando expire o choque

        yield return new WaitForSeconds(2); // esperar lo que tardaria en desaparecer

        Destroy(gameObject);
    }

    IEnumerator Collision()
    {
        // hacer que choquen las estrellas por vfx

        yield return new WaitForSeconds(_explosionDelay); // esperar lo que tardarian en chocar

        // stop o apagar vfx principal y prender explosion
        float timer = 0;
        List<Collider> ignore = new List<Collider>();
        ignore.Add(player.GetComponent<Collider>());
        bool skip = false;

        while (timer < _explosionDuration)
        {
            var cols = Physics.OverlapSphere(transform.position, _explosionRadius);

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

        yield return new WaitForSeconds(2); // esperar lo que tardaria en desaparecer la explosion

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

        StartCoroutine(Death());
    }
}
