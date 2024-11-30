using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TutorialManager : MonoBehaviour
{
    public bool inProgress = false;

    public void StartTutorial()
    {
        inProgress = true;

        GameManager.instance.player.Inputs.inputUpdate = GameManager.instance.player.Inputs.MoveToGetUp;

        // prender ui wasd to move
    }
}
