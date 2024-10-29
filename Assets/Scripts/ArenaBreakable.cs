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

        if (_hp <= 0) StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        _breakVFX.enabled=true;
        GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(1.5f);
        Die();
    }

    public void Die()
    {
        

        Destroy(gameObject);
    }
}
