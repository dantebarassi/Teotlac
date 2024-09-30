using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialObsidianTrap : SpecialMagic
{
    ObsidianTrap _trap;
    float _shardDamage, _shardSpeed, _preparation, _recovery, _cooldown;

    public SpecialObsidianTrap(PlayerController player, Inputs inputs, ObsidianTrap trap, float cost, float damage, float shardSpeed, float preparation, float recovery, float cooldown)
    {
        _player = player;
        _inputs = inputs;
        _trap = trap;
        _staminaCost = cost;
        _shardDamage = damage;
        _shardSpeed = shardSpeed;
        _preparation = preparation;
        _recovery = recovery;
        _cooldown = cooldown;
    }

    public override bool Activate(out float cooldown)
    {
        _player.StartCoroutine(PlaceTrap());
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

    IEnumerator PlaceTrap()
    {
        _inputs.inputUpdate = _inputs.FixedCast;

        yield return new WaitForSeconds(_preparation);

        var trap = Object.Instantiate(_trap, _player.transform.position - Vector3.up * _player.DistToFloor + _player.transform.forward * 1.5f, _player.transform.rotation);
        trap.shardSpeed = _shardSpeed;
        trap.shardDamage = _shardDamage;

        yield return new WaitForSeconds(_recovery);

        _inputs.inputUpdate = _inputs.Unpaused;
    }
}
