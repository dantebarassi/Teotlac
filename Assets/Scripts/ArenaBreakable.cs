using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ArenaBreakable : MonoBehaviour, IDamageable
{
    [SerializeField] float _hp;

    VisualEffect _breakVFX;

    private void Start()
    {
        _breakVFX = GetComponent<VisualEffect>();
        _breakVFX.SetMesh("Mesh", GetComponent<MeshFilter>().mesh);
    }

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        _hp -= amount;

        if (_hp <= 0) Die();
    }

    public void Die()
    {
        _breakVFX.Play();

        Destroy(gameObject);
    }
}
