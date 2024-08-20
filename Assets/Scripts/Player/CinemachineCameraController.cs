using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineCameraController : MonoBehaviour
{
    public static CinemachineCameraController instance;

    [SerializeField] CinemachineFreeLook _freeLookCamera;
    [SerializeField] CinemachineVirtualCamera _aimCamera;

    public CinemachineVirtualCamera AimCamera
    {
        get
        {
            return _aimCamera;
        }
    }

    public CinemachineFreeLook FreeLookCamera
    {
        get
        {
            return _freeLookCamera;
        }
    }

    [SerializeField] Transform _aimAxis;

    [SerializeField] PlayerController _player;

    [SerializeField] float _aimSensitivity, _minAimRotation, _maxAimRotation;

    float _mouseX, _mouseY;

    CinemachineBasicMultiChannelPerlin _freeNoise, _aimNoise;

    bool _turnPlayer = false;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);

        _mouseX = _player.transform.eulerAngles.y;
        instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        _aimNoise = _aimCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        _freeNoise = _freeLookCamera.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void UpdateCameraRotation(float xAxi, float yAxi)
    {
        if (xAxi == 0 && yAxi == 0) return;

        if (xAxi != 0 && _turnPlayer)
        {
            _mouseX += xAxi * _aimSensitivity * Time.deltaTime;

            if (_mouseX > 360 || _mouseX < -360)
            {
                _mouseX -= 360 * Mathf.Sign(_mouseX);
            }

            _player.transform.eulerAngles = new Vector3(0, _mouseX);
        }

        if (yAxi != 0)
        {
            _mouseY += yAxi * _aimSensitivity * Time.deltaTime;

            _mouseY = Mathf.Clamp(_mouseY, _minAimRotation, _maxAimRotation);
        }

        _aimAxis.eulerAngles = new Vector3(-_mouseY, _mouseX);
    }

    public void Final()
    {
        _freeLookCamera.gameObject.SetActive(false);
        _aimCamera.gameObject.SetActive(false);
    }

    public void FreeLook()
    {
        _turnPlayer = false;

        _freeLookCamera.enabled = true;
        _aimCamera.enabled = false;

        if(_mouseX < 0) _freeLookCamera.m_XAxis.Value = _mouseX.Remap(-360, 0, -180, 180);
        else _freeLookCamera.m_XAxis.Value = _mouseX.Remap(0, 360, -180, 180);

        _freeLookCamera.m_YAxis.Value = Mathf.InverseLerp(_minAimRotation, _maxAimRotation, -_mouseY);
    }

    public void Aim()
    {
        _mouseX = _player.transform.eulerAngles.y;
        _aimAxis.eulerAngles = new Vector3(-_mouseY, _mouseX);
        _turnPlayer = true;

        _freeLookCamera.enabled = false;
        _aimCamera.enabled = true;
    }

    public void CameraShake(float intensity, float duration)
    {
        StartCoroutine(Shaking(intensity, duration));
    }

    private IEnumerator Shaking(float intensity, float duration)
    {
        _freeNoise.m_AmplitudeGain = intensity;
        _aimNoise.m_AmplitudeGain = intensity;

        yield return new WaitForSeconds(duration);

        _freeNoise.m_AmplitudeGain = 0;
        _aimNoise.m_AmplitudeGain = 0;
    }
}
