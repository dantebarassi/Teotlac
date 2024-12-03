using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TutorialManager : MonoBehaviour
{
    public bool inProgress = false;

    [SerializeField] TutorialDummy _dummy;
    [SerializeField] int _basicHitCount, _finisherHitCount, _explosionHitCount;

    public void StartTutorial()
    {
        inProgress = true;

        GameManager.instance.player.Inputs.inputUpdate = GameManager.instance.player.Inputs.MoveToGetUp;

        StartCoroutine(WASD());
    }

    IEnumerator WASD()
    {
        yield return new WaitForSeconds(3);

        UIManager.instance.ChangeText(true, "WASD to move, Space to jump");
    }

    public void GotUp()
    {
        UIManager.instance.ChangeText(true, "F to Interact");
    }

    public void TeachBasic()
    {
        GameManager.instance.player.Inputs.inputUpdate = GameManager.instance.player.Inputs.JustBasicAttack;

        _dummy.currentAction = _dummy.Moving;
        _dummy.checkingFor = TutorialDummy.PossibleAttacks.Basic;

        UIManager.instance.ChangeText(true, "Left click to use the basic attack. Holding A or D will make you sidestep as you fire. Use it on the dummy " + _basicHitCount + " times");
    }

    public void SuccessfulBasic()
    {
        _basicHitCount--;
        Debug.Log("succesful");
        UIManager.instance.ChangeText(true, "Left click to use the basic attack. Holding A or D will make you sidestep as you fire. Use it on the dummy " + _basicHitCount + " times");

        if (_basicHitCount <= 0) TeachFinisher();
    }

    public void TeachFinisher()
    {
        GameManager.instance.player.Inputs.inputUpdate = GameManager.instance.player.Inputs.BasicAndFinisher;

        _dummy.checkingFor = TutorialDummy.PossibleAttacks.Finisher;

        UIManager.instance.ChangeText(true, "After using at least 2 basics, right click to use a stronger attack. Use it on the dummy " + _finisherHitCount + " times");
    }

    public void SuccessfulFinisher()
    {
        _finisherHitCount--;

        UIManager.instance.ChangeText(true, "After using at least 2 basics, right click to use a stronger attack. Use it on the dummy " + _finisherHitCount + " times");

        if (_finisherHitCount <= 0) TeachRoll();
    }

    public void TeachRoll()
    {
        GameManager.instance.player.Inputs.inputUpdate = GameManager.instance.player.Inputs.NoSpecials;

        _dummy.checkingFor = TutorialDummy.PossibleAttacks.None;
        _dummy.currentAction = _dummy.MovingAndShooting;

        UIManager.instance.ChangeText(true, "Shift to roll and gain invincibility for a short time");

        StartCoroutine(RollTimer());
    }

    IEnumerator RollTimer()
    {
        yield return new WaitForSeconds(12);

        TeachStarCollision();
    }

    public void TeachStarCollision()
    {
        GameManager.instance.player.Inputs.inputUpdate = GameManager.instance.player.Inputs.StarCollision;

        _dummy.checkingFor = TutorialDummy.PossibleAttacks.StarCollision;

        UIManager.instance.ChangeText(true, "Q to use Star Collision. Must be charged without taking damage. Once charged, Q to throw and Q again to detonate");
    }

    public void SuccessfulExplosion()
    {
        _explosionHitCount--;

        if (_explosionHitCount <= 0) TeachNebulaShield();
    }

    public void TeachNebulaShield()
    {
        GameManager.instance.player.Inputs.inputUpdate = GameManager.instance.player.Inputs.Unpaused;

        _dummy.checkingFor = TutorialDummy.PossibleAttacks.None;
        _dummy.currentAction = _dummy.Shooting;

        UIManager.instance.ChangeText(true, "E to use Nebula Shield. Fires a counter after absorbing a set amount of damage. E again to counter early");

        StartCoroutine(ShieldTimer());
    }

    IEnumerator ShieldTimer()
    {
        yield return new WaitForSeconds(12);

        UIManager.instance.ChangeText(true, "Interact with the statue to return to the world and viceversa. Leave whenever you are ready");
        _dummy.currentAction = _dummy.MovingAndShooting;

        yield return new WaitForSeconds(5);

        UIManager.instance.ChangeText(false);
        inProgress = false;
        GameManager.instance.Json.saveData.finishedTutorial = true;
    }
}
