using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ObsidianGroundSpikes : MonoBehaviour
{
    [SerializeField] VisualEffect _spikesVFX;
    [SerializeField] Rigidbody _rb;

    ObjectPool<ObsidianGroundSpikes> _objectPool;

    float _speed, _damage, _indestructibleTime;

    bool _dead = false;

    public void SetUpVFX(float size, float lifetime)
    {
        _spikesVFX.SetFloat("Radiu 2", size);
        _spikesVFX.SetFloat("SizeZ", size);
        _spikesVFX.SetFloat("Lifetime", lifetime);
    }

    public void Initialize(ObjectPool<ObsidianGroundSpikes> pool, Vector3 pos, Quaternion rotation, float speed, float damage)
    {
        _objectPool = pool;
        transform.position = pos;
        transform.rotation = rotation;
        _speed = speed;
        _damage = damage;
        _indestructibleTime = 0.04f;

        _spikesVFX.Play();
    }

    private void Update()
    {
        if (_indestructibleTime > 0)
        {
            _indestructibleTime -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (_dead) return;

        _rb.MovePosition(transform.position + _speed * Time.fixedDeltaTime * transform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_dead) return;

        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage);
        }

        if (_indestructibleTime > 0) return;

        StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        _spikesVFX.Stop();
        _dead = true;

        yield return new WaitForSeconds(1.5f);

        _objectPool.RefillStock(this);
    }

    public static void TurnOff(ObsidianGroundSpikes x)
    {
        x.gameObject.SetActive(false);
    }

    public static void TurnOn(ObsidianGroundSpikes x)
    {
        x.gameObject.SetActive(true);
    }
}
