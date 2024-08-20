using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource au;
    [SerializeField] AudioClip auC;
    public static SoundManager instance;
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
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void ChangeAudioBossFight()
    {
        au.clip = auC;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
