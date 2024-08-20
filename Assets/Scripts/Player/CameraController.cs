using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera _myCamera;

    [Header("Target")]
    [SerializeField] Transform _myTarget;

    [Header("Mouse Settings")]
    [SerializeField] float _mouseSensitivity = 100;

    [Header("Camera Distance")]
    [Range(0.25f, 2f), SerializeField]
    float _minDistance;

    [Range(2f, 10f), SerializeField]
    float _maxDistance;

    [Header("Clamp Settings")]
    [SerializeField] float _minRotation;
    [SerializeField] float _maxRotation;

    [Header("Raycast Settings")]
    [SerializeField] float _hitOffset;

    float _mouseX, _mouseY;

    Vector3 _direction;
    Vector3 _camPos;

    Vector3 _shakeModifier;

    Ray _ray;
    RaycastHit _rHit;
    bool _isCameraBlocked;

    void Start()
    {
        _myCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;

        transform.forward = _myTarget.forward;

        _mouseX = transform.eulerAngles.y;
        _mouseY = transform.eulerAngles.x;
    }

    private void FixedUpdate()
    {
        _ray = new Ray(transform.position, _direction);

        _isCameraBlocked = Physics.SphereCast(_ray, 0.1f, out _rHit, _maxDistance);
    }

    public void StartShake(float magnitude, float duration)
    {
        StartCoroutine(CameraShake(magnitude, duration));
    }

    IEnumerator CameraShake(float magnitude, float duration)
    {
        Vector3 orignalPosition = transform.position;
        float time = 0f;

        while (time < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            _shakeModifier = transform.right * x + transform.up * y;
            time += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        transform.position = orignalPosition;
        _shakeModifier = Vector3.zero;
    }

    public void FreeLook(float x, float y)
    {
        UpdateCameraRotation(x, y);
        UpdateSpringArm();
    }

    public void LockedOn(Vector3 target)
    {
        LockedOnTo(target);
        UpdateSpringArm(target);
    }

    public void UpdateCameraRotation(float xAxi, float yAxi)
    {
        transform.position = _myTarget.position;

        if (xAxi == 0 && yAxi == 0) return;

        if (xAxi != 0)
        {
            _mouseX += xAxi * _mouseSensitivity * Time.deltaTime;

            if (_mouseX > 360 || _mouseX < -360)
            {
                _mouseX -= 360 * Mathf.Sign(_mouseX);
            }
        }

        if (yAxi != 0)
        {
            _mouseY += yAxi * _mouseSensitivity * Time.deltaTime;

            _mouseY = Mathf.Clamp(_mouseY, _minRotation, _maxRotation);
        }

        transform.rotation = Quaternion.Euler(-_mouseY, _mouseX, 0);
    }

    void UpdateSpringArm()
    {
        _direction = -transform.forward;

        if (_isCameraBlocked)
        {
            var dirTest = (_rHit.point - transform.position) + (_rHit.normal * _hitOffset);

            if (dirTest.sqrMagnitude <= _minDistance * _minDistance)
            {
                _camPos = transform.position + _direction * _minDistance;
            }
            else
            {
                _camPos = transform.position + dirTest;
            }
        }
        else
        {
            _camPos = transform.position + _direction * _maxDistance;
        }

        _myCamera.transform.position = _camPos + _shakeModifier;
        //Y le digo a mi camara que mire hacia el personaje
        _myCamera.transform.LookAt(transform.position + _shakeModifier);
    }

    void UpdateSpringArm(Vector3 LookAt)
    {
        _direction = -transform.forward;

        if (_isCameraBlocked)
        {
            var dirTest = (_rHit.point - transform.position) + (_rHit.normal * _hitOffset);

            if (dirTest.sqrMagnitude <= _minDistance * _minDistance)
            {
                _camPos = transform.position + _direction * _minDistance;
            }
            else
            {
                _camPos = transform.position + dirTest;
            }
        }
        else
        {
            _camPos = transform.position + _direction * _maxDistance;
        }

        _myCamera.transform.position = _camPos + _shakeModifier;
        //Y le digo a mi camara que mire hacia el personaje
        _myCamera.transform.LookAt(LookAt + _shakeModifier);
    }

    public void LockedOnTo(Vector3 target)
    {
        transform.position = _myTarget.position;
        transform.LookAt(target - Vector3.up * Vector3.Distance(transform.position, target) * 0.5f);
    }
}
