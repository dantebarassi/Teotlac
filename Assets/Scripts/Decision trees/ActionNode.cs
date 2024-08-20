using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionNode : BehaviorNode
{
    public ActionNode(Action action)
    {
        _action = action;
    }

    Action _action;

    public override void Execute()
    {
        _action();
    }
}
