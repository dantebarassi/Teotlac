using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QuestionNode : BehaviorNode
{
    public QuestionNode(BehaviorNode trueNode, BehaviorNode falseNode, Func<bool> question)
    {
        _trueNode = trueNode;
        _falseNode = falseNode;
        _question = question;
    }

    BehaviorNode _trueNode;
    BehaviorNode _falseNode;

    Func<bool> _question;

    public override void Execute()
    {
        ExecuteNextNode(_question());
    }

    public void ExecuteNextNode(bool question)
    {
        if (question == true) _trueNode.Execute();
        else _falseNode.Execute();
    }
}
