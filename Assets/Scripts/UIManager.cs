using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField] Slider _hpBar, _staminaBar, _bossHpBar;
    [SerializeField] Button _joystick;
    [SerializeField] Image _paused, _black;
    [SerializeField] GameObject _uiParent, _sunActive, _obsidianActive;
    [SerializeField] Image[] _specials = new Image[2];
    [SerializeField] Image[] _specialsCooldowns = new Image[2];
    [SerializeField] GameObject _crosshair, _options, mainMenu;
    [SerializeField] TextMeshProUGUI _bossName, _tutorialText;

    public GameObject textoFinal;

    public enum Bar
    {
        PlayerHp,
        PlayerStamina,
        BossHp
    }

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);

        instance = this;
    }

    public void ChangeBossName(string name)
    {
        _bossName.text = name;
    }

    public void HideUI(bool hide)
    {
        _uiParent.SetActive(!hide);
    }

    public void UpdateBar(Bar bar, float newValue)
    {
        switch (bar)
        {
            case Bar.PlayerHp:
                _hpBar.value = newValue;
                break;
            case Bar.PlayerStamina:
                _staminaBar.value = newValue;
                break;
            case Bar.BossHp:
                _bossHpBar.value = newValue;
                break;
            default:
                break;
        }
    }
    public void UpdateBar(Bar bar, float newValue, float maxValue)
    {
        switch (bar)
        {
            case Bar.PlayerHp:
                _hpBar.maxValue = maxValue;
                _hpBar.value = newValue;
                break;
            case Bar.PlayerStamina:
                _staminaBar.maxValue = maxValue;
                _staminaBar.value = newValue;
                break;
            case Bar.BossHp:
                _bossHpBar.maxValue = maxValue;
                _bossHpBar.value = newValue;
                break;
            default:
                break;
        }
    }

    public void UpdateBasicSpell(PlayerController.MagicType type)
    {
        switch (type)
        {
            case PlayerController.MagicType.Sun:
                _sunActive.SetActive(true);
                _obsidianActive.SetActive(false);
                break;
            case PlayerController.MagicType.Obsidian:
                _sunActive.SetActive(false);
                _obsidianActive.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void UpdateSpecialIcon(int number, Sprite icon)
    {
        _specials[number].sprite = icon;
        _specialsCooldowns[number].sprite = icon;
    }

    public void UpdateSpecialCooldown(int number, float value)
    {
        _specialsCooldowns[number].fillAmount = value;
    }

    public void ToggleCrosshair(bool turnOn)
    {
        _crosshair.SetActive(turnOn);
    }

    public void ToggleBossBar(bool turnOn)
    {
        _bossHpBar.gameObject.SetActive(turnOn);
    }

    public void ChangeText(bool show)
    {
        _tutorialText.gameObject.SetActive(show);
    }

    public void ChangeText(bool show, string newText)
    {
        _tutorialText.gameObject.SetActive(show);
        _tutorialText.text = newText;
    }

    public void Fade(bool into)
    {
        if (into)
        {
            StartCoroutine(FadeToBlack());
        }
        else
        {
            StartCoroutine(FadeFromBlack());
        }
    }

    IEnumerator FadeToBlack()
    {
        float timer = 0;

        while (timer < 1)
        {
            timer += Time.deltaTime;

            _black.color = Color.black - new Color(0, 0, 0, Mathf.Lerp(1, 0, timer));

            yield return null;
        }
    }

    IEnumerator FadeFromBlack()
    {
        float timer = 0;

        while (timer < 1)
        {
            timer += Time.deltaTime;

            _black.color = Color.black - new Color(0, 0, 0, Mathf.Lerp(0, 1, timer));

            yield return null;
        }
    }

    public void StartPlaceholderDemoEnd()
    {
        StartCoroutine(PlaceholderDemoEnd());
    }

    IEnumerator PlaceholderDemoEnd()
    {
        StartCoroutine(FadeToBlack());

        yield return new WaitForSeconds(2);

        ChangeText(true, "Final de la demo, gracias por jugar");
    }

    public void Paused()
    {
        _paused.gameObject.SetActive(true);
        _joystick.gameObject.SetActive(true);
    }
    public void UnPaused()
    {
        _paused.gameObject.SetActive(false);
        _joystick.gameObject.SetActive(false);
    }
    public void Options()
    {
        _options.gameObject.SetActive(true);
        _paused.gameObject.SetActive(false);
        _joystick.gameObject.SetActive(false);
    }


    public void Final()
    {
        _hpBar.gameObject.SetActive(false);
        _staminaBar.gameObject.SetActive(false);
        textoFinal.SetActive(true);
    }
}
