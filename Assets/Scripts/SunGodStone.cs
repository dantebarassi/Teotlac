using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunGodStone : MonoBehaviour, IInteractable
{
    [SerializeField] Dummy _dummy;
    [SerializeField] Transform _dummySpawnPos;
    [SerializeField] GameObject _camera;
    [SerializeField] float _emissiveIntensity;
    [SerializeField] Light _light;
    [Range(0, 8)]
    [SerializeField] float _lightIntensity;
    [SerializeField] float _transitionDuration;
    [SerializeField] TutorialManager _tutorial;
    [SerializeField] TutorialDummy _tutorialDummy;

    Collider _collider;
    Material _material;
    
    bool _training = false, _startedTutorial;

    PlayerController _player;

    private void Start()
    {
        _material = GetComponent<Renderer>().material;
        _collider = GetComponent<Collider>();
    }

    public void Interact(PlayerController player)
    {
        _player = player;

        if (_tutorial.inProgress)
        {
            if (!_startedTutorial)
            {
                _startedTutorial = true;

                _tutorialDummy.gameObject.SetActive(true);
                _tutorialDummy.transform.position = _dummySpawnPos.position;
                _tutorialDummy.transform.rotation = _dummySpawnPos.rotation;
                _tutorial.TeachBasic();

                _player.FightStarts(_tutorialDummy);
            }

            // que cuente un poco de la historia, hacer dialogos clickeando para pasar al siguiente

            return;
        }

        if (!_training)
        {
            _camera.SetActive(true);

            _collider.enabled = false;

            _player.Inputs.inputUpdate = _player.Inputs.Nothing;

            StartCoroutine(ToggleLight(true, _transitionDuration));

            UIManager.instance.ToggleSunGodInteraction(true);
            UIManager.instance.HideUI(true);
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            DespawnDummy();
        }
    }

    public void GoBack()
    {
        _camera.SetActive(false);

        _collider.enabled = true;

        _player.Inputs.inputUpdate = _player.Inputs.Unpaused;

        StartCoroutine(ToggleLight(false, _transitionDuration));

        UIManager.instance.ToggleSunGodInteraction(false);
        UIManager.instance.HideUI(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Train()
    {
        _camera.SetActive(false);

        _training = true;

        _collider.enabled = true;

        _player.Inputs.inputUpdate = _player.Inputs.Unpaused;

        StartCoroutine(ToggleLight(false, _transitionDuration));

        UIManager.instance.ToggleSunGodInteraction(false);
        UIManager.instance.HideUI(false);
        Cursor.lockState = CursorLockMode.Locked;

        SpawnDummy();
    }

    void SpawnDummy()
    {
        _dummy.gameObject.SetActive(true);
        _dummy.transform.position = _dummySpawnPos.position;
        _dummy.transform.rotation = _dummySpawnPos.rotation;

        _player.FightStarts(_dummy);
    }

    public void DespawnDummy()
    {
        _training = false;

        if (_player != null) _player.FightEnds();

        _dummy.gameObject.SetActive(false);
        _tutorialDummy.gameObject.SetActive(false);
    }

    IEnumerator ToggleLight(bool on, float duration)
    {
        float timer = 0, lerpT;
        
        if (on)
        {
            while (timer < duration)
            {
                timer += Time.deltaTime;

                lerpT = timer / duration;

                _material.SetFloat("_Emission_Intensity", Mathf.Lerp(0, _emissiveIntensity, lerpT));
                _light.intensity = Mathf.Lerp(0, _lightIntensity, lerpT);

                yield return null;
            }
        }
        else
        {
            while (timer < duration)
            {
                timer += Time.deltaTime;

                lerpT = timer / duration;

                _material.SetFloat("_Emission_Intensity", Mathf.Lerp(_emissiveIntensity, 0, lerpT));
                _light.intensity = Mathf.Lerp(_lightIntensity, 0, lerpT);

                yield return null;
            }
        }
    }
}
