using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ArenaBreakable : MonoBehaviour, IDamageable
{
    [SerializeField] float _hp;
    float _currentHp;
    [SerializeField] VisualEffect _breakVFX;

    Collider _collider;
    MeshRenderer _renderer;
    bool _damaged = false, _dead = false;

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();

        _currentHp = _hp;
    }

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        _currentHp -= amount;

        if (!_damaged && _currentHp <= _hp * 0.5f)
        {
            _damaged = true;

            _renderer.material.SetInt("_Cracks", 1);
        }

        if (! _dead && _currentHp <= 0)
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
