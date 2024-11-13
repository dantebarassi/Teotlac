using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject _options, _main;

    public void Main()
    {
        _main.gameObject.SetActive(true);
        _options.gameObject.SetActive(false);
    }

    public void Options()
    {
        _options.gameObject.SetActive(true);
        _main.gameObject.SetActive(false);
    }

    public void Play()
    {
        SceneLoader.instance.LoadLevel("TenoParaCine");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
