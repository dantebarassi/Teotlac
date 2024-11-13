using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    [SerializeField] GameObject _loadScreen;
    [SerializeField] Image _black, _loadingBar;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
    }

    public void LoadMenu()
    {
        StartCoroutine(LoadingLevel("SceneSelector", false));

        Time.timeScale = 1;
    }

    public void LoadLevel(string name)
    {
        StartCoroutine(LoadingLevel(name));

        Time.timeScale = 1;
    }

    public void LoadLevel(int index)
    {
        StartCoroutine(LoadingLevel(index));

        Time.timeScale = 1;
    }

    IEnumerator LoadingLevel(string name, bool cursorLocked = true)
    {
        float timer = 0;

        while (timer < 1)
        {
            timer += Time.deltaTime;

            _black.color = Color.black - new Color(0, 0, 0, Mathf.Lerp(1, 0, timer));

            yield return null;
        }

        _loadScreen.SetActive(true);

        AsyncOperation load = SceneManager.LoadSceneAsync(name);

        while (!load.isDone)
        {
            _loadingBar.fillAmount = Mathf.Clamp01(load.progress / 0.9f);

            yield return null;
        }

        _loadScreen.SetActive(false);

        if (!cursorLocked) Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator LoadingLevel(int index)
    {
        float timer = 0;

        while (timer < 1)
        {
            timer += Time.deltaTime;

            _black.color = Color.black - new Color(0, 0, 0, Mathf.Lerp(1, 0, timer));

            yield return null;
        }

        _loadScreen.SetActive(true);

        AsyncOperation load = SceneManager.LoadSceneAsync(index);

        while (!load.isDone)
        {
            _loadingBar.fillAmount = Mathf.Clamp01(load.progress / 0.9f);

            yield return null;
        }

        _loadScreen.SetActive(false);
    }
}
