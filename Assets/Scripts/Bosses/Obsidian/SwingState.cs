using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingState : State
{
    public SwingState(ObsidianGod boss, Animator anim)
    {
        _boss = boss;
        _anim = anim;
    }

    public override void OnEnter()
    {
        _boss.renderer.material.color = Color.red;
        _anim.SetBool("IsDashAttack", true);
        _boss.Swing();
    }

    public override void OnExit()
    {
        _anim.SetBool("IsDashAttack", false);
    }

    public override void OnUpdate()
    {
        if (!_boss.takingAction)
        {
            fsm.ChangeState(_boss.GetAction());
        }
    }
}
