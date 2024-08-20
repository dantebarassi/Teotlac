using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShardsState : State
{
    public ShardsState(ObsidianGod boss, Animator anim)
    {
        _boss = boss;
        _anim = anim;
    }

    public override void OnEnter()
    {
        _boss.renderer.material.color = Color.grey;
        _anim.SetBool("IsShooting", true);
        _boss.Shards(3);
    }

    public override void OnExit()
    {
        _anim.SetBool("IsShooting", false);
    }

    public override void OnUpdate()
    {
        if (!_boss.takingAction)
        {
            fsm.ChangeState(_boss.GetAction());
        }
    }
}
