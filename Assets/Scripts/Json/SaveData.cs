using System;
using System.Collections.Generic;
using UnityEngine;


//Es una variable que se le pueden guardar datos toda la clase
[Serializable]
public class SaveData
{
    public int actualBoss = 0;
    public Vector3 lastCheckPoingPosition;
    public bool finishedTutorial = false;
}
