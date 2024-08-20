using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveState : State
{
    public WaveState(ObsidianGod boss, Animator anim)
    {
        _boss = boss;
        _anim = anim;
    }

    public override void OnEnter()
    {
        _boss.renderer.material.color = Color.yellow;
        _anim.SetBool("IsBoxAttack", true);
        _boss.Wave();
    }

    public override void OnExit()
    {
        _anim.SetBool("IsBoxAttack", false);
    }

    public override void OnUpdate()
    {
        if (!_boss.takingAction)
        {
            fsm.ChangeState(_boss.GetAction());
        }
    }
}
