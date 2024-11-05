using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ArenaBreakable : MonoBehaviour, IDamageable
{
    [SerializeField] float _hp;
    float _currentHp;
    [SerializeField] VisualEffect _breakVFX;
    [SerializeField] GameObject _brokenPhase;
    [SerializeField] List<GameObject> _fragments;
    Collider _collider;
    MeshRenderer _renderer;
    bool _broken = false, _damaged = false, _dead = false;

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();

        _currentHp = _hp;
    }

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        _currentHp -= amount;

        if (_brokenPhase != null && !_broken && _currentHp <= _hp * 0.6f)
        {
            _broken = true;

            Break();
        }

        if (!_damaged && _currentHp <= _hp * 0.3f)
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

    void Break()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        _brokenPhase.SetActive(true);

        StartCoroutine(DestroyFragments());
    }

    IEnumerator DestroyFragments()
    {
        yield return new WaitForSeconds(5);

        foreach (var item in _fragments)
        {
            Destroy(item);
        }
    }

    IEnumerator Death()
    {
        var vfx = Instantiate(_breakVFX, transform.position, transform.rotation);
        if (!_broken) vfx.SetMesh("Mesh", GetComponent<MeshFilter>().mesh);
        else vfx.SetMesh("Mesh", _brokenPhase.GetComponent<MeshFilter>().mesh);
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
