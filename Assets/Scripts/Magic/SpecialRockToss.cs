using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialRockToss : SpecialMagic
{
    Rock _rock;
    Transform _spawnPos;
    float _damage, _strength, _angle, _preparation, _recovery, _cooldown;

    public SpecialRockToss(PlayerController player, Inputs inputs, Rock rock, Transform spawnPos, float cost, float damage, float strength, float angle, float preparation, float recovery, float cooldown)
    {
        _player = player;
        _inputs = inputs;
        _rock = rock;
        _spawnPos = spawnPos;
        _staminaCost = cost;
        _damage = damage;
        _strength = strength;
        _angle = angle;
        _preparation = preparation;
        _recovery = recovery;
        _cooldown = cooldown;
    }

    public override bool Activate(out float cooldown)
    {
        _player.StartCoroutine(Toss());
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

    IEnumerator Toss()
    {
        _inputs.inputUpdate = _inputs.FixedCast;

        yield return new WaitForSeconds(_preparation);

        var rock = Object.Instantiate(_rock, _spawnPos.position, _player.transform.rotation);
        rock.transform.eulerAngles += new Vector3(-_angle, 0);
        rock.Toss(rock.transform.forward, _strength, _damage);

        yield return new WaitForSeconds(_recovery);

        _inputs.inputUpdate = _inputs.Unpaused;
    }
}
