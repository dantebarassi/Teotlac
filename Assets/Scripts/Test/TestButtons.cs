using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButtons : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Joystick1Button1))
            Debug.Log("Toco la x");
        if (Input.GetKey(KeyCode.Joystick1Button0))
            Debug.Log("Toco la cuadrado");
        if (Input.GetKey(KeyCode.Joystick1Button2))
            Debug.Log("Toco la circulo");
        if (Input.GetKey(KeyCode.Joystick1Button3))
            Debug.Log("Toco la triangulo");
        if (Input.GetKey(KeyCode.Joystick1Button5))
            Debug.Log("Toco la R1");
        if (Input.GetKey(KeyCode.Joystick1Button7))
            Debug.Log("Toco la R2");
        if (Input.GetKey(KeyCode.Joystick1Button9))
            Debug.Log("Toco la Options");
    }
}
