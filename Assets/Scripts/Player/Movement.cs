using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement
{
    public delegate void FloatsDelegate(float a, float b);
    //public event FloatsDelegate OnRotation;

    float _currentSpeed, _normalSpeed, _explorationSpeed, _currentMoveSpeed, _speedOnCast, _turnRate, _jumpStrength, _currentStepStrength, _stepStrength, _castStepStrength;
    Transform _playerTransform;
    Rigidbody _rb;
    LayerMask _groundLayer;
    PlayerController _player;

    public Movement(Transform transform, Rigidbody rigidbody, float speed, float explorationSpeed, float speedOnCast, float turnRate, float jumpStrength, float stepStrength, float castStepStrength, LayerMask groundLayer)
    {
        _playerTransform = transform;
        _rb = rigidbody;
        _currentSpeed = explorationSpeed;
        _normalSpeed = speed;
        _explorationSpeed = explorationSpeed;
        _turnRate = turnRate;
        _speedOnCast = speedOnCast;
        _jumpStrength = jumpStrength;
        _currentStepStrength = stepStrength;
        _stepStrength = stepStrength;
        _castStepStrength = castStepStrength;
        _groundLayer = groundLayer;
        _currentMoveSpeed = explorationSpeed;
        _player = _playerTransform.gameObject.GetComponent<PlayerController>();
    }

    /*public void Move(float horizontalInput, float verticalInput, bool changeForward)
    {
        if (horizontalInput == 0 && verticalInput == 0) return;

        var dir = GetDir(horizontalInput, verticalInput);

        if(changeForward)
            _playerTransform.forward = dir;

        _rb.MovePosition(_playerTransform.position + _currentSpeed * Time.fixedDeltaTime * dir);
    }*/

    public void Move(float horizontalInput, float verticalInput, bool changeForward)
    {
        if (horizontalInput == 0 && verticalInput == 0)
        {
            _player.RunningAnimation(false);
            if (changeForward)
            {
                _rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            return;
        }

        _player.RunningAnimation(true);

        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        var dir = GetDir(horizontalInput, verticalInput);

        if (changeForward)
        {
            if (_playerTransform.forward != dir)
            {
                var eulerRotation = _playerTransform.rotation.eulerAngles;

                var yRotation = Vector3.Angle(_playerTransform.right, dir) < Vector3.Angle(-_playerTransform.right, dir) ? _turnRate : -_turnRate;

                var angleToDesired = Vector3.Angle(_playerTransform.forward, dir);

                yRotation *= Time.fixedDeltaTime;
                var absYRotation = Mathf.Abs(yRotation);

                if (angleToDesired > absYRotation)
                {
                    _rb.MoveRotation(Quaternion.Euler(eulerRotation.x, eulerRotation.y + yRotation, eulerRotation.z));

                    if (angleToDesired - absYRotation > 20) return;
                }
                else
                {
                    _playerTransform.forward = dir;
                }
            }
        }
            
        _rb.MovePosition(_playerTransform.position + _currentSpeed * Time.fixedDeltaTime * dir);
    }

    public void Cast(bool starts)
    {
        if (starts)
        {
            _currentSpeed = _speedOnCast;
            _currentStepStrength = _castStepStrength;
        }
        else
        {
            _currentSpeed = _currentMoveSpeed;
            _currentStepStrength = _stepStrength;
        }
    }

    public void Jump()
    {
        _rb.AddForce(Vector3.up * _jumpStrength);
    }

    public void Step(float horizontalInput, float verticalInput)
    {
        Vector3 dir, cameraForward;

        if (Mathf.Abs(horizontalInput) < 0.5f && Mathf.Abs(verticalInput) < 0.5f)
        {
            cameraForward = Camera.main.transform.forward.MakeHorizontal();
            dir = -cameraForward;
            _playerTransform.forward = cameraForward;
        }
        else
        {
            dir = GetDir(horizontalInput, verticalInput, out cameraForward);
        }

        dir.Normalize();

        _rb.velocity = Vector3.zero;
        _rb.AddForce(dir * _currentStepStrength);
    }

    public bool IsGrounded()
    {
        return Physics.BoxCast(_playerTransform.position + Vector3.up, new Vector3(0.25f, 0.1f, 0.25f), -_playerTransform.up, Quaternion.identity, 1, _groundLayer);
    }

    public float DistanceToFloor()
    {
        Physics.Raycast(_playerTransform.position, -_playerTransform.up, out var hit, Mathf.Infinity, _groundLayer);

        return hit.distance;
    }

    public void Combat(bool starts)
    {
        _currentMoveSpeed = starts ? _normalSpeed : _explorationSpeed;
        _currentSpeed = _currentMoveSpeed;
    }

    Vector3 GetDir(float hInput, float vInput)
    {
        var cameraForward = Camera.main.transform.forward.MakeHorizontal();
        var cameraRight = Camera.main.transform.right.MakeHorizontal();

        Vector3 direction = cameraForward * vInput + cameraRight * hInput;

        if (direction.sqrMagnitude > 1)
        {
            direction.Normalize();
        }

        return direction;
    }

    Vector3 GetDir(float hInput, float vInput, out Vector3 cameraForward)
    {
        cameraForward = Camera.main.transform.forward.MakeHorizontal();
        var cameraRight = Camera.main.transform.right.MakeHorizontal();

        Vector3 direction = cameraForward * vInput + cameraRight * hInput;

        if (direction.sqrMagnitude > 1)
        {
            direction.Normalize();
        }

        return direction;
    }
}
