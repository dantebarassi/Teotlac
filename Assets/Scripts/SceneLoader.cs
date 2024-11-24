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
    [SerializeField] float _minLoadDuration;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
    }

    public void LoadMenu()
    {
        StartCoroutine(LoadingLevel("SceneSelectorNew", _minLoadDuration, false));

        Time.timeScale = 1;
    }

    public void LoadLevel(string name)
    {
        StartCoroutine(LoadingLevel(name, _minLoadDuration));

        Time.timeScale = 1;
    }

    public void LoadLevel(int index)
    {
        StartCoroutine(LoadingLevel(index, _minLoadDuration));

        Time.timeScale = 1;
    }

    IEnumerator LoadingLevel(string name, float minDuration, bool cursorLocked = true)
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

        load.allowSceneActivation = false;

        timer = 0;

        while (!load.isDone && timer < minDuration)
        {
            timer += Time.deltaTime;

            _loadingBar.fillAmount = Mathf.Clamp(load.progress / 0.9f, 0, timer / minDuration); ;

            yield return null;
        }

        if (!cursorLocked) Cursor.lockState = CursorLockMode.None;

        load.allowSceneActivation = true;
    }

    IEnumerator LoadingLevel(int index, float minDuration)
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

        load.allowSceneActivation = false;

        timer = 0;

        while (!load.isDone && timer < minDuration)
        {
            timer += Time.deltaTime;

            _loadingBar.fillAmount = Mathf.Clamp(load.progress / 0.9f, 0, timer / minDuration); ;

            yield return null;
        }

        load.allowSceneActivation = true;
    }
}
