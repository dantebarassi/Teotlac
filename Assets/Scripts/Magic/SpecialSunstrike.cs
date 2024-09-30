using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialSunstrike : SpecialMagic
{
    GameObject _firstRay, _secondRay;
    AudioSource _audioSource;
    AudioClip _audioClip;
    float _damage, _radius, _preparation, _delay, _linger, _cooldown;

    public SpecialSunstrike(PlayerController player, Inputs inputs, GameObject firstRay, GameObject secondRay, AudioSource audio, AudioClip clip, float cost, float damage, float radius, float preparation, float delay, float linger, float cooldown)
    {
        _firstRay = firstRay;
        _secondRay = secondRay;
        _audioSource = audio;
        _audioClip = clip;
        _player = player;
        _inputs = inputs;
        _staminaCost = cost;
        _damage = damage;
        _radius = radius;
        _preparation = preparation;
        _delay = delay;
        _linger = linger;
        _cooldown = cooldown;
    }

    public override bool Activate(out float cooldown)
    {
        if (_player.currentBoss != null)
        {
            _player.StartCoroutine(Sunstriking());
            cooldown = _cooldown;
            return true;
        }
        else
        {
            cooldown = 0;
            return false;
        }
    }

    public override float ReturnCost()
    {
        return _staminaCost;
    }

    IEnumerator Sunstriking()
    {
        var boss = _player.currentBoss;

        _inputs.inputUpdate = _inputs.FixedCast;
        _player.transform.forward = boss.transform.position - _player.transform.position;
        _player.anim.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(_preparation);

        _player.anim.SetBool("IsAttacking", false);
        _inputs.inputUpdate = _inputs.Unpaused;

        if (!boss) yield break;

        Vector3 strikePos = boss.transform.position - Vector3.up * boss.DistFromPivotToFloor;
        var ray1 = Object.Instantiate(_firstRay, strikePos, Quaternion.identity);
        var audioSource = Object.Instantiate(_audioSource, strikePos, Quaternion.identity);
        audioSource.clip = _audioClip;
        audioSource.Play();

        yield return new WaitForSeconds(_delay);

        Object.Destroy(ray1);
        var ray2 = Object.Instantiate(_secondRay, strikePos, Quaternion.identity);
        ray2.transform.localScale = new Vector3(_radius, ray2.transform.localScale.y, _radius);

        if (Vector3.Distance(boss.transform.position, strikePos) <= _radius)
        {
            boss.TakeDamage(_damage);
        }

        CinemachineCameraController.instance.CameraShake(3, _linger);

        yield return new WaitForSeconds(_linger);

        Object.Destroy(audioSource);
        Object.Destroy(ray2);
    }
}
