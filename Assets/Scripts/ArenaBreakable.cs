using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ArenaBreakable : MonoBehaviour, IDamageable
{
    [SerializeField] float _hp;
    [SerializeField] VisualEffect _breakVFX;

    Collider _collider;
    MeshRenderer _renderer;
    bool _damaged = false, _dead = false;

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();
    }

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        _hp -= amount;

        if (!_damaged && _hp <= 0)
        {
            _damaged = true;

            // hacer que parezca medio roto
        }

        if (! _dead && _hp <= 0)
        {
            _dead = true;
            
            Die();
        }
    }

    IEnumerator Death()
    {
        var vfx = Instantiate(_breakVFX, transform.position, transform.rotation);
        vfx.SetMesh("Mesh", GetComponent<MeshFilter>().mesh);
        vfx.Play();

        _collider.enabled = false;
        _renderer.enabled = false;

        yield return new WaitForSeconds(1.5f);
        
        Destroy(gameObject);
    }

    public void Die()
    {
        StartCoroutine(Death());
    }
}
