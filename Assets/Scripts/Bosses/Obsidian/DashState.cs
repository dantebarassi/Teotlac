using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : State
{
    public DashState(ObsidianGod boss, Animator anim)
    {
        _boss = boss;
        _anim = anim;
    }

    public override void OnEnter()
    {
        _boss.renderer.material.color = Color.blue;
        _anim.SetBool("IsDashing", true);
        _boss.Dash();
    }

    public override void OnExit()
    {
        _anim.SetBool("IsDashing", false);
    }

    public override void OnUpdate()
    {
        if (!_boss.takingAction)
        {
            fsm.ChangeState(_boss.GetAction());
        }
    }
}
