using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialFirewall : SpecialMagic
{
    GameObject _firewall;
    float _preparation, _recovery, _cooldown;

    public SpecialFirewall(PlayerController player, Inputs inputs, GameObject firewall, float preparation, float recovery, float cooldown)
    {
        _player = player;
        _inputs = inputs;
        _firewall = firewall;
        _preparation = preparation;
        _recovery = recovery;
        _cooldown = cooldown;
    }

    public override float Activate()
    {
        _player.StartCoroutine(Firewalling());

        return _cooldown;
    }

    IEnumerator Firewalling()
    {
        var camForward = Camera.main.transform.forward.MakeHorizontal();

        _inputs.inputUpdate = _inputs.FixedCast;
        _player.transform.forward = camForward;
        //_player.anim.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(_preparation);

        Object.Instantiate(_firewall, _player.transform.position + camForward * 1.5f - Vector3.up, _player.transform.rotation);

        yield return new WaitForSeconds(_recovery);

        _inputs.inputUpdate = _inputs.Unpaused;
    }
}
