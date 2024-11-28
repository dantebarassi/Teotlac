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
    Collider _collider, _brokenCollider;
    MeshRenderer _renderer, _brokenRenderer;
    AudioSource _audioSource;
    bool _broken = false, _damaged = false, _dead = false;

    private void Start()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<MeshRenderer>();
        _audioSource = GetComponent<AudioSource>();

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

            if (_broken) _brokenRenderer.material.SetInt("_Cracks", 1);
            else _renderer.material.SetInt("_Cracks", 1);
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

        _brokenCollider = _brokenPhase.GetComponent<Collider>();
        _brokenRenderer = _brokenPhase.GetComponent<MeshRenderer>();

        _audioSource.PlayOneShot(AudioManager.instance.StructureBreak());

        StartCoroutine(DestroyFragments());
    }

    IEnumerator DestroyFragments()
    {
        List<Material> fragmentMats = new();

        foreach (var item in _fragments)
        {
            fragmentMats.Add(item.GetComponent<Renderer>().material);
        }

        yield return new WaitForSeconds(4);

        float timer = 0;

        while (timer < 1.75f)
        {
            timer += Time.deltaTime;

            foreach (var item in fragmentMats)
            {
                item.SetFloat("_Alpha", Mathf.Lerp(1, 0, timer / 1.5f));
            }

            yield return null;
        }

        foreach (var item in _fragments)
        {
            Destroy(item);
        }
    }

    IEnumerator Death()
    {
        yield return new WaitForSeconds(0.02f);

        var vfx = Instantiate(_breakVFX, transform.position, transform.rotation);
        if (!_broken)
        {
            vfx.SetMesh("Mesh", GetComponent<MeshFilter>().mesh);

            _collider.enabled = false;
            _renderer.enabled = false;
        }
        else
        {
            vfx.SetMesh("Mesh", _brokenPhase.GetComponent<MeshFilter>().mesh);

            _brokenRenderer.enabled = false;
            _brokenCollider.enabled = false;
        }
        
        vfx.Play();

        _audioSource.PlayOneShot(AudioManager.instance.StructureBreak());

        yield return new WaitForSeconds(6);
        
        Destroy(gameObject);
    }

    public void Die()
    {
        StartCoroutine(Death());
    }
}
