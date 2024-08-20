using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inputs
{
    public System.Action inputUpdate, cameraInputs, previousUpdate;
    float _inputHorizontal, _inputVertical;
    float _inputMouseX, _inputMouseY;
    Movement _movement;
    PlayerController _player;
    CinemachineCameraController _cameraController;
    bool _jump, _primaryAttack = false, _secondaryAttack = false, _aiming = false;

    public bool trigger = false;

    KeyCode _kStep, _kJump, _kPrimaryAttack, _kSecondaryAttack, _kSpecial1, _kSpecial2, _kSun, _kObsidian, _kPause;

    public bool PrimaryAttack
    {
        get
        {
            return _primaryAttack;
        }

        set
        {
            if (value == !_primaryAttack)
            {
                _primaryAttack = value;

                if (value)
                {
                    previousUpdate = inputUpdate;
                    _player.ActivateMagic();
                }
                else
                {
                    inputUpdate = previousUpdate;
                }
            }
        }
    }

    public bool SecondaryAttack
    {
        get
        {
            return _secondaryAttack;
        }

        set
        {
            if (value == !_secondaryAttack)
            {
                _secondaryAttack = value;

                if (value)
                {
                    previousUpdate = inputUpdate;
                    _player.ActivateSecondaryMagic();
                }
                else
                {
                    inputUpdate = previousUpdate;
                }
            }
        }
    }

    public bool launchAttack = false;

    public Inputs(Movement movement, PlayerController player, CinemachineCameraController camera)
    {
        _movement = movement;
        _player = player;
        _cameraController = camera;
        FreeLook();
        cameraInputs = CameraInputsMouse;
    }


    public void CameraInputsMouse()
    {
        _inputMouseX = Input.GetAxisRaw("Mouse X");

        _inputMouseY = Input.GetAxisRaw("Mouse Y");
    }
    public void CameraInputsJoystick()
    {
        _inputMouseX = Input.GetAxisRaw("Mouse X");

        _inputMouseY = Input.GetAxisRaw("Mouse Y");
    }

    public void Altern(bool joystick)
    {
        if(joystick)
        {
            cameraInputs = CameraInputsJoystick;
            _kStep = KeyCode.Joystick1Button2;
            _kJump = KeyCode.Joystick1Button1;
            _kPrimaryAttack = KeyCode.Joystick1Button7;
            _kSecondaryAttack = KeyCode.Joystick1Button6;
            _kSpecial1 = KeyCode.Q; // L1
            _kSpecial2 = KeyCode.E; // R1
            _kSun = KeyCode.Joystick1Button4;
            _kObsidian = KeyCode.Joystick1Button4;
            _kPause = KeyCode.Joystick1Button9;
        }
        else
        {
            cameraInputs = CameraInputsMouse;
            _kStep = KeyCode.LeftShift;
            _kJump = KeyCode.Space;
            _kPrimaryAttack = KeyCode.Mouse0;
            _kSecondaryAttack = KeyCode.Mouse1;
            _kSpecial1 = KeyCode.Q;
            _kSpecial2 = KeyCode.E;
            _kSun = KeyCode.Alpha1;
            _kObsidian = KeyCode.Alpha2;
            _kPause = KeyCode.Escape;
        }
        
    }

    public void WaitUntilAim()
    {
        if (Input.GetKeyDown(_kSecondaryAttack))
        {
            inputUpdate = JustBasicAttack;
            SecondaryAttack = true;
            trigger = true;
        }
    }

    public void WaitUntilFirstSpecial()
    {
        if (Input.GetKeyDown(_kSpecial1))
        {
            _player.UseSpecial(0);
            trigger = true;
        }
    }

    public void WaitUntilSecondSpecial()
    {
        if (Input.GetKeyDown(_kSpecial2))
        {
            _player.UseSpecial(1);
            trigger = true;
        }
    }

    public void NoAttackInputs()
    {
        cameraInputs();

        _inputHorizontal = Input.GetAxis("Horizontal");

        _inputVertical = Input.GetAxis("Vertical");

        Pause(NoAttackInputs);

        if (Input.GetKeyDown(_kStep))
        {
            _player.Step(_inputHorizontal, _inputVertical);

            PrimaryAttack = false;
        }

        if (Input.GetKeyDown(_kJump))
        {
            _jump = true;
        }
    }

    public void JustBasicAttack()
    {
        cameraInputs();

        _inputHorizontal = Input.GetAxis("Horizontal");

        _inputVertical = Input.GetAxis("Vertical");

        Pause(JustBasicAttack);

        if (Input.GetKeyDown(_kStep))
        {
            _player.Step(_inputHorizontal, _inputVertical);

            PrimaryAttack = false;
        }

        if (Input.GetKeyDown(_kJump))
        {
            _jump = true;
        }

        if (Input.GetKeyDown(_kPrimaryAttack))
        {
            PrimaryAttack = true;
        }

        if (Input.GetKeyUp(_kPrimaryAttack))
        {
            PrimaryAttack = false;
        }

        if (Input.GetKeyDown(_kSecondaryAttack))
        {
            SecondaryAttack = true;
        }
    }

    public void NoJump()
    {
        cameraInputs();

        _inputHorizontal = Input.GetAxis("Horizontal");

        _inputVertical = Input.GetAxis("Vertical");

        Pause(NoJump);

        if (Input.GetKeyDown(_kStep))
        {
            _player.Step(_inputHorizontal, _inputVertical);

            PrimaryAttack = false;
        }

        if (Input.GetKeyDown(_kPrimaryAttack))
        {
            PrimaryAttack = true;
        }

        if (Input.GetKeyUp(_kPrimaryAttack))
        {
            PrimaryAttack = false;
        }

        if (Input.GetKeyDown(_kSecondaryAttack))
        {
            SecondaryAttack = true;
        }
    }

    public void Unpaused()
    {
        cameraInputs();

        _inputHorizontal = Input.GetAxis("Horizontal");

        _inputVertical = Input.GetAxis("Vertical");

        Pause(Unpaused);

        if (Input.GetKeyDown(_kStep))
        {
            _player.Step(_inputHorizontal, _inputVertical);

            PrimaryAttack = false;
        }

        if (Input.GetKeyDown(_kJump))
        {
            _jump = true;
        }

        if (Input.GetKeyDown(_kPrimaryAttack))
        {
            PrimaryAttack = true;
        }

        if (Input.GetKeyUp(_kPrimaryAttack))
        {
            PrimaryAttack = false;
        }

        if (Input.GetKeyDown(_kSecondaryAttack))
        {
            SecondaryAttack = true;
        }

        if (Input.GetKeyDown(_kSpecial1))
        {
            _player.UseSpecial(0);
        }

        if (Input.GetKeyDown(_kSpecial2))
        {
            _player.UseSpecial(1);
        }

        //AimUnaim();

        SelectSun();

        SelectObsidian();
    }

    public void Stepping()
    {
        _inputHorizontal = 0;

        _inputVertical = 0;

        cameraInputs();

        Pause(Stepping);

        SelectSun();

        SelectObsidian();

        //AimUnaim();
    }

    public void FixedCast()
    {
        _inputHorizontal = 0;

        _inputVertical = 0;

        cameraInputs();

        //if (Mathf.Abs(Input.GetAxis("Horizontal")) == 1 || Mathf.Abs(Input.GetAxis("Vertical")) == 1)
        //{
        //    Attack = false;
        //    inputUpdate = Unpaused;
        //}

        Pause(FixedCast);

        /*if (Input.GetKeyDown(_Kstep))
        {
            _player.Step(_inputHorizontal, _inputVertical);
            PrimaryAttack = false;
            inputUpdate = Unpaused;
        }

        if (Input.GetKeyDown(_Kjump))
        {
            _jump = true;
            PrimaryAttack = false;
            inputUpdate = Unpaused;
        }

        if (Input.GetKeyUp(_KprimaryAttack))
        {
            PrimaryAttack = false;
            inputUpdate = Unpaused;
        }

        AimUnaim();*/
    }

    //public void MovingCast()
    //{
    //    _inputHorizontal = Input.GetAxis("Horizontal");
    //
    //    _inputVertical = Input.GetAxis("Vertical");
    //
    //    cameraInputs();
    //
    //    Pause();
    //
    //    if (Input.GetKeyDown(KeyCode.LeftShift))
    //    {
    //        _player.Step(_inputHorizontal, _inputVertical);
    //        _player.StopSun = true;
    //        PrimaryAttack = false;
    //    }
    //
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        _jump = true;
    //        _player.StopSun = true;
    //        PrimaryAttack = false;
    //    }
    //
    //    if (Input.GetKeyUp(_kPrimaryAttack))
    //    {
    //        PrimaryAttack = false;
    //    }
    //
    //    //AimUnaim();
    //}

    public void Aiming()
    {
        _inputHorizontal = Input.GetAxis("Horizontal");

        _inputVertical = Input.GetAxis("Vertical");

        cameraInputs();

        Pause(Aiming);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _player.Step(_inputHorizontal, _inputVertical);
            _player.StopChannels = true;
            SecondaryAttack = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jump = true;
            _player.StopChannels = true;
            SecondaryAttack = false;
        }

        if (Input.GetKeyDown(_kPrimaryAttack))
        {
            Debug.Log(SecondaryAttack + "lol");
            launchAttack = true;
        }

        if (Input.GetKeyUp(_kSecondaryAttack))
        {
            SecondaryAttack = false;
        }

        //AimUnaim();
    }

    public void Nothing()
    {
        _inputHorizontal = 0;

        _inputVertical = 0;
    }

    public void Paused()
    {
        if (Input.GetKeyDown(_kPause))
        {
            Time.timeScale = 1;
            UIManager.instance.UnPaused();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            //UIManager.instance.SetPauseMenu(false);
            inputUpdate = previousUpdate;
        }
    }

    public void InputsFixedUpdate()
    {
        _movement.Move(_inputHorizontal, _inputVertical, !_player.Aiming);

        if (_jump)
        {
            _player.Jump();
            _jump = false;
        }
    }

    public void InputsLateUpdate()
    {
        _cameraController.UpdateCameraRotation(_inputMouseX, _inputMouseY);
    }

    public void ToggleAim(bool aim)
    {
        //_aiming = aim;

        if (aim)
        {
            _player.transform.forward = Camera.main.transform.forward.MakeHorizontal();
            Aim();
        }
        else FreeLook();

        UIManager.instance.ToggleCrosshair(aim);
    }

    void FreeLook()
    {
        _cameraController.FreeLook();
    }

    void Aim()
    {
        _cameraController.Aim();
    }

    void Pause(System.Action prePause)
    {
        if (Input.GetKeyDown(_kPause))
        {
            previousUpdate = prePause;
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            //UIManager.instance.SetPauseMenu(true);
            UIManager.instance.Paused();
            inputUpdate = Paused;
        }
    }

    //void AimUnaim()
    //{
    //    if (Input.GetKeyDown(_Kaim))
    //    {
    //        ToggleAim();
    //    }
    //}

    void SelectSun()
    {
        if (Input.GetKeyDown(_kSun))
        {
            _player.ChangeActiveMagic(PlayerController.MagicType.Sun);
            _player.renderer.material.color = Color.red;
        }
    }

    void SelectObsidian()
    {
        if (Input.GetKeyDown(_kObsidian))
        {
            _player.ChangeActiveMagic(PlayerController.MagicType.Obsidian);
            _player.renderer.material.color = Color.black;
        }
    }
}
