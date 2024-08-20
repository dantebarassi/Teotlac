using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesState : State
{
    public SpikesState(ObsidianGod boss, Animator anim)
    {
        _boss = boss;
        _anim = anim;
    }

    public override void OnEnter()
    {
        _boss.renderer.material.color = Color.black;
        _anim.SetBool("IsStomp", true);
        _boss.Spikes();
    }

    public override void OnExit()
    {
        _anim.SetBool("IsStomp", false);
    }

    public override void OnUpdate()
    {
        if (!_boss.takingAction)
        {
            fsm.ChangeState(_boss.GetAction());
        }
    }
}
