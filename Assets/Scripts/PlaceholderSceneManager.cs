using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlaceholderSceneManager : MonoBehaviour
{
    [SerializeField] GameObject _options, mainMenu;
    [SerializeField] Image _black;

    public void StartFadeToBlack()
    {
        StartCoroutine(FadeToBlack());
    }

    IEnumerator FadeToBlack()
    {
        float timer = 0;

        while(timer < 1)
        {
            timer += Time.deltaTime;

            _black.color = Color.black - new Color(0, 0, 0, Mathf.Lerp(1, 0, timer));

            yield return null;
        }

        ChangeScene("TenoParacine");
    }

    public void MainMenu()
    {
        mainMenu.gameObject.SetActive(true);
        _options.gameObject.SetActive(false);
    }
    public void Options()
    {
        _options.gameObject.SetActive(true);
        mainMenu.gameObject.SetActive(false);
    }
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
    public void ChangeScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
