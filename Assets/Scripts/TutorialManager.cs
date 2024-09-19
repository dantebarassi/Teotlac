using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] GameObject _camera;
    [SerializeField] PlayableDirector _introCinematic, _bossCinematic;
    [SerializeField] PlayerController _player;
    [SerializeField] ObsidianGod _enemy;
    [SerializeField] string _movement, _jump, _step, _sun, _supernova, _sunstrike, _switch;
    [SerializeField] Collider[] _colliders;
    [SerializeField] float _firstWait, _firstSectionDelays, _preFightWait, _supernovaWait, _sunstrikeWait;

    int _colliderCounter = 0;

    private void Start()
    {
        UIManager.instance.BlackScreenFade(false);

        if (!GameManager.instance.playIntro) 
        {
            _camera.SetActive(false);
            return;
        } 

        _introCinematic.Play();

        UIManager.instance.HideUI(true);

        StartCoroutine(FirstSection());
    }

    IEnumerator FirstSection()
    {
        _player.Inputs.inputUpdate = _player.Inputs.Nothing;

        yield return new WaitForSeconds(_firstWait);

        _player.Inputs.inputUpdate = _player.Inputs.Unpaused;
        UIManager.instance.HideUI(false);

        GameManager.instance.playIntro = false;
        //_player.Inputs.inputUpdate = _player.Inputs.NoAttackInputs;
        //
        //UIManager.instance.ChangeText(true, _movement);
        //
        //yield return new WaitForSeconds(_firstSectionDelays);
        //
        //UIManager.instance.ChangeText(true, _jump);
        //
        //yield return new WaitForSeconds(_firstSectionDelays);
        //
        //UIManager.instance.ChangeText(true, _step);
        //
        //yield return new WaitForSeconds(_firstSectionDelays);
        //
        //UIManager.instance.ChangeText(false);
    }

    IEnumerator PreFight()
    {
        yield return new WaitForSeconds(_preFightWait);

        UIManager.instance.ChangeText(true, _sun);
        Time.timeScale = 0;
        _player.Inputs.inputUpdate = _player.Inputs.WaitUntilAim;

        while (!_player.Inputs.trigger)
        {
            yield return null;
        }

        _player.Inputs.trigger = false;
        UIManager.instance.ChangeText(false);
        Time.timeScale = 1;
    }

    IEnumerator Specials()
    {
        _enemy.BreakWall();

        yield return new WaitForSeconds(_supernovaWait);

        UIManager.instance.ChangeText(true, _supernova);
        Time.timeScale = 0;
        _player.Inputs.inputUpdate = _player.Inputs.WaitUntilSecondSpecial;

        while (!_player.Inputs.trigger)
        {
            yield return null;
        }
        
        _player.Inputs.trigger = false;
        UIManager.instance.ChangeText(false);
        Time.timeScale = 1;

        yield return new WaitForSeconds(_sunstrikeWait);

        UIManager.instance.ChangeText(true, _sunstrike);

        while (_enemy != null)
        {
            yield return null;
        }

        UIManager.instance.ChangeText(false);
    }

    IEnumerator PostFight()
    {
        UIManager.instance.ChangeText(true, _switch);

        yield return new WaitForSeconds(_firstSectionDelays);

        UIManager.instance.ChangeText(false);
    }

    IEnumerator MainFightStart()
    {
        UIManager.instance.HideUI(true);
        _player.Cutscene(true);
        _bossCinematic.Play();

        yield return new WaitWhile(() => _bossCinematic.state == PlayState.Playing);

        _player.Cutscene(false);
        UIManager.instance.HideUI(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            _colliders[0].enabled = false;
            StartCoroutine(MainFightStart());
        }
    }
}
