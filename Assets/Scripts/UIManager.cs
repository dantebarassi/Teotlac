using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField] Slider _hpBar, _staminaBar, _bossHpBar;
    [SerializeField] Button _joystick;
    [SerializeField] Image _paused, _black, _noStamina, _tookDamage;
    [SerializeField] GameObject _uiParent, _sunActive, _obsidianActive, _lowHp, _fButton, _sunGodInteraction;
    [SerializeField] Image[] _specials = new Image[2];
    [SerializeField] Image[] _specialsCooldown = new Image[2];
    [SerializeField] Image[] _specialsUnavailable = new Image[2];
    [SerializeField] Image[] _specialsFramesCooldown = new Image[2];
    [SerializeField] Image[] _specialsFramesUnavailable = new Image[2];
    [SerializeField] GameObject _crosshair, _options, mainMenu;
    [SerializeField] TextMeshProUGUI _bossName, _tutorialText;

    bool _showingNoStamina = false;
    bool[] _showingSpecialUnavailable = new bool[2];

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
        _specialsCooldown[number].sprite = icon;
    }

    public void UpdateSpecialCooldown(int number, float value)
    {
        _specialsCooldown[number].fillAmount = value;
        _specialsFramesCooldown[number].fillAmount = value;
    }

    public void SpecialUnavailable(int number)
    {
        if (_showingSpecialUnavailable[number]) return;

        StartCoroutine(FadeToggleImage(_specialsUnavailable[number], 0.1f, 0.2f, 0.3f, 1, false, number));
        StartCoroutine(FadeToggleImage(_specialsFramesUnavailable[number], 0.1f, 0.2f, 0.3f, 1, false, number));
    }

    public void ToggleSunGodInteraction(bool turnOn)
    {
        _sunGodInteraction.SetActive(turnOn);
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

    public void BlackScreenFade(bool into)
    {
        StartCoroutine(FadeImage(_black, 1, into));
    }

    IEnumerator FadeImage(Image image, float time, bool unfade = false, float alphaValue = 1)
    {
        float timer = 0;
        var startingColor = image.color;

        while (timer < time)
        {
            timer += Time.deltaTime;

            image.color = unfade ? startingColor + new Color(0, 0, 0, Mathf.Lerp(0, alphaValue, timer / time)) : startingColor - new Color(0, 0, 0, Mathf.Lerp(0, alphaValue, timer / time));

            yield return null;
        }
    }

    IEnumerator FadeToggleImage(Image image, float inDuration, float wait, float outDuration, float alphaValue = 1, bool isStamina = false, int isSpecial = -1)
    {
        if (isStamina) _showingNoStamina = true;
        if (isSpecial != -1) _showingSpecialUnavailable[isSpecial] = true;

        StartCoroutine(FadeImage(image, inDuration, true, alphaValue));

        yield return new WaitForSeconds(wait);

        StartCoroutine(FadeImage(image, outDuration, false, alphaValue));

        yield return new WaitForSeconds(outDuration);

        if (isStamina) _showingNoStamina = false;
        if (isSpecial != -1) _showingSpecialUnavailable[isSpecial] = false;
    }

    public void NotEnoughStamina()
    {
        if (_showingNoStamina) return;

        StartCoroutine(FadeToggleImage(_noStamina, 0.1f, 0.2f, 0.3f, 0.6f, true));
    }

    public void TookDamage()
    {
        StartCoroutine(FadeToggleImage(_tookDamage, 0.1f, 0.2f, 0.3f, 0.2f));
    }

    public void LowHp()
    {
        _lowHp.SetActive(true);
    }

    public void ToggleInteractable(bool on)
    {
        _fButton.SetActive(on);
    }

    public void StartPlaceholderDemoEnd()
    {
        StartCoroutine(PlaceholderDemoEnd());
    }

    IEnumerator PlaceholderDemoEnd()
    {
        yield return new WaitForSeconds(3);

        ChangeText(true, "Final de la demo, gracias por jugar");

        yield return new WaitForSeconds(4);

        Cursor.lockState = CursorLockMode.None;

        SceneLoader.instance.LoadMenu();
    }

    public void Paused()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _paused.gameObject.SetActive(true);
        _joystick.gameObject.SetActive(true);
    }
    public void UnPaused()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _paused.gameObject.SetActive(false);
        _joystick.gameObject.SetActive(false);
    }
    public void Options()
    {
        _options.gameObject.SetActive(true);
        _paused.gameObject.SetActive(false);
        _joystick.gameObject.SetActive(false);
    }
}
