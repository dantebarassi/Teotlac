using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.Rendering.Universal;
//using UnityEngine.InputSystem;

public class ControlFullScreen : MonoBehaviour
{
    public static ControlFullScreen instance;
    [SerializeField] private ControlFullScreen _fullScreenControl;
    [SerializeField] private Material _material;

    private int _vignetteIntensity = Shader.PropertyToID("_VignetteAmount");
    // Start is called before the first frame update
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
    void Start()
    {
        _fullScreenControl.gameObject.SetActive(true);
    }

    public void ChangeDemond(bool activate)
    {
        if (activate)
        {
            _material.SetFloat(_vignetteIntensity, 0.873f);
        }
        else
        {
            _material.SetFloat(_vignetteIntensity, 1f);
        }
    }
}
