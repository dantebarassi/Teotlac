using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool joystick=false, playerInPalace, hasCheckpoint;
    [SerializeField] private Material _vignettePostProcess;
    [SerializeField] private string _vignetteAmountName = "";
    public float aaaaa;
    public Vector3 playerWorldPos = Vector3.zero;
    public Light sunLight;
    public bool playIntro;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
