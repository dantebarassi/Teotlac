using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialSupernova : SpecialMagic
{
    GameObject _supernova;
    float _radius, _damage, _preparation, _duration, _recovery, _cooldown;

    public SpecialSupernova(PlayerController player, Inputs inputs, GameObject supernova, float cost, float radius, float damage, float preparation, float duration, float recovery, float cooldown)
    {
        _player = player;
        _inputs = inputs;
        _supernova = supernova;
        _staminaCost = cost;
        _radius = radius;
        _damage = damage;
        _preparation = preparation;
        _duration = duration;
        _recovery = recovery;
        _cooldown = cooldown;
    }

    public override bool Activate(out float cooldown)
    {
        _player.StartCoroutine(Supernova());
        cooldown = _cooldown;
        return true;
    }

    public override bool AltActivate(out float cooldown)
    {
        return Activate(out cooldown);
    }

    public override float ReturnCost()
    {
        return _staminaCost;
    }

    IEnumerator Supernova()
    {
        _inputs.inputUpdate = _inputs.FixedCast;
        _player.anim.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(_preparation);

        var nova = Object.Instantiate(_supernova, _player.transform.position, Quaternion.identity);
        nova.transform.localScale *= _radius;

        float timer = 0;
        List<Collider> ignore = new List<Collider>();
        ignore.Add(_player.GetComponent<Collider>());
        bool skip = false;

        while (timer < _duration)
        {
            var cols = Physics.OverlapSphere(_player.transform.position, _radius);

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
                    damageable.TakeDamage(_damage);

                    if (item != null) ignore.Add(item);
                }
                else
                {
                    damageable = item.GetComponentInParent<IDamageable>();

                    if (damageable != null) damageable.TakeDamage(_damage);

                    if (item != null) ignore.Add(item);
                }
            }

            timer += Time.deltaTime;

            yield return null;
        }

        Object.Destroy(nova);

        yield return new WaitForSeconds(_recovery);

        _player.anim.SetBool("IsAttacking", false);
        _inputs.inputUpdate = _inputs.Unpaused;
    }
}
