using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : Entity
{
    Movement _movement;
    Inputs _inputs;

    public Inputs Inputs
    {
        get
        {
            return _inputs;
        }
    }

    [SerializeField] float _maxStamina, _staminaRegenRate, _staminaRegenDelay, _damageCooldown, _speed, _explorationSpeed, _speedOnCast, _turnRate, _jumpStr, _stepStr, _castStepStr, _stepCooldown/*(variable del step viejo)_stepStopVelocity*/;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] Transform _aimTarget;

    [Header("Stamina costs")]
    [SerializeField] float _jumpCost;
    [SerializeField] float _stepCost, _sunBaseCost, _sunHoldCost, _obsidianCost;

    [Header("Sun Magic")]
    [SerializeField] SunMagic _sunMagic;
    [SerializeField] Transform _sunSpawnPoint;
    [SerializeField] float _sunBaseDamage, _sunDamageGrowRate, _sunSpeed, _sunMaxChargeTime, _sunCastDelay, _sunShootDelay, _sunRecovery, _sunCooldown, _sunAbsorbTime, _sunMeleeDuration,_sunHitboxX, _sunHitboxY, _sunHitboxZ, _sunRange;
    Vector3 _sunHitbox;

    [Header("Obsidian Magic")]
    [SerializeField] PlayerProjectile _obsidianShard;
    [SerializeField] Transform _obsidianSpawnPoint;
    [SerializeField] float _obsidianDamage, _obsidianAimDelay, _obsidianShotDelay, _obsidianComboInterval, _obsidianCooldown, _shardMaxAngleVariation, _shardSpeed, _shardDuration;
    [SerializeField] int _obsidianShardAmount, _maxWaves;

    float _stepCurrentCooldown = 0, _obsidianCurrentCooldown = 0, _sunCurrentCooldown = 0, _damageCurrentCooldown = 0;

    float _stamina, _currentStaminaDelay = 0;

    public Boss currentBoss;

    MagicType _activeMagic;

    [SerializeField] CinemachineCameraController _cameraController;

    public GameObject camaraFinal;

    public Renderer renderer;
    private bool _joystickActive=true, _aiming = false, _stopChannels = false;

    [SerializeField] Material _VignetteAmountClamps;

    [SerializeField] ParticleSystem dustPlayer;
    [SerializeField] GameObject ForceFieldPlayer;

    SpecialsManager _specials;

    public bool StopChannels
    {
        set
        {
            _stopChannels = value;
        }
    }

    public bool Aiming
    {
        get
        {
            return _aiming;
        }
    }

    public bool Grounded
    {
        get
        {
            return _movement.IsGrounded();
        }
    }

    public float DistToFloor
    {
        get
        {
            return _movement.DistanceToFloor();
        }
    }

    public Animator anim;

    AudioSource _myAS;
    [SerializeField] AudioClip sideStep,jump, damage, chargingSun, Walking;
    [SerializeField] GameObject _stepParticles, _jumpParticles;

    public enum MagicType
    {
        Sun,
        Obsidian
    }

    protected override void Awake()
    {
        base.Awake();
        _myAS = GetComponent<AudioSource>();
        _movement = new Movement(transform, _rb, _speed, _explorationSpeed, _speedOnCast, _turnRate, _jumpStr, _stepStr, _castStepStr, _groundLayer);
        _inputs = new Inputs(_movement, this, _cameraController);
        _specials = GetComponent<SpecialsManager>();

        _activeMagic = MagicType.Sun;
        renderer.material.color = Color.red;
        _sunHitbox = new Vector3(_sunHitboxX, _sunHitboxY, _sunHitboxZ);
        _inputs.inputUpdate = _inputs.Unpaused;
    }

    void Start()
    {
        UIManager.instance.UpdateBar(UIManager.Bar.PlayerHp, _hp, _maxHp);

        _stamina = _maxStamina;
        UIManager.instance.UpdateBar(UIManager.Bar.PlayerStamina, _stamina, _maxStamina);

        Joistick();
    }

    void Update()
    {
        if (transform.position.y < -87) Die();

        if (_inputs.inputUpdate != null) _inputs.inputUpdate();

        ManageCooldowns();

        StaminaRegeneration();
    }

    private void FixedUpdate()
    {
        _inputs.InputsFixedUpdate();
    }

    private void LateUpdate()
    {
        _inputs.InputsLateUpdate();
    }

    public void Jump()
    {
        if (Grounded && _stepCurrentCooldown <= _stepCooldown * 0.5f && CheckAndReduceStamina(_jumpCost))
        {
            //StartCoroutine(ToggleGameObject(_jumpParticles));
            anim.SetTrigger("jump");
            _movement.Jump();
            ChangeAudio(jump);
            //anim.SetBool("IsJumping", false);
        }
    }

    public void Step(float horizontalInput, float verticalInput)
    {
        if (Grounded && _stepCurrentCooldown <= 0 && CheckAndReduceStamina(_stepCost))
        {
            anim.SetTrigger("step");
            //StartCoroutine(ToggleGameObject(_stepParticles));
            //anim.SetBool("IsStrafeRight", true);
            ChangeAudio(sideStep);
            _stepCurrentCooldown = _stepCooldown;
            _movement.Step(horizontalInput, verticalInput);
        }
    }

    public void UseSpecial(int slot)
    {
        if (Grounded && _specials.IsOffCooldown(slot) && CheckAndReduceStamina(_specials.GetCost(slot)))
        {
            _specials.ActivateSpecial(slot);
        }
    }

    public void ChangeActiveMagic(MagicType type)
    {
        _activeMagic = type;
        UIManager.instance.UpdateBasicSpell(type);
        switch (type)
        {
            case MagicType.Sun:
                _specials.EquipSpecial(SpecialsManager.Specials.Sunstrike, 0);
                _specials.EquipSpecial(SpecialsManager.Specials.Supernova, 1);
                break;
            case MagicType.Obsidian:
                _specials.EquipSpecial(SpecialsManager.Specials.ObsidianTrap, 0);
                _specials.EquipSpecial(SpecialsManager.Specials.RockToss, 1);
                break;
            default:
                break;
        }
    }

    public void ActivateMagic()
    {
        switch (_activeMagic)
        {
            case MagicType.Sun:
                //ActivateSunMagic();
                break;
            case MagicType.Obsidian:
                ActivateObsidianMagic();
                break;
        }
    }

    public void ActivateSecondaryMagic()
    {
        switch (_activeMagic)
        {
            case MagicType.Sun:
                ActivateAimedSunMagic();
                break;
            case MagicType.Obsidian:
                ActivateAimedObsidianMagic();
                break;
        }
    }

    void ActivateSunMagic()
    {
        if (_movement.IsGrounded() && _sunCurrentCooldown <= 0 && CheckAndReduceStamina(_sunBaseCost))
        {
            //StartCoroutine(NewSunMagic());
        }
        else
        {
            _inputs.PrimaryAttack = false;
            //_inputs.SecondaryAttack = false;
        }
    }

    void ActivateAimedSunMagic()
    {
        if (!_aiming && _movement.IsGrounded() && _sunCurrentCooldown <= 0 && CheckAndReduceStamina(_sunBaseCost))
        {
            StartCoroutine(AimedSunMagic());
        }
        else
        {
            //_inputs.PrimaryAttack = false;
            _inputs.SecondaryAttack = false;
        }
    }

    IEnumerator AimedSunMagic()
    {
        _rb.angularVelocity = Vector3.zero;
        _inputs.ToggleAim(true);
        _aiming = true;
        _inputs.inputUpdate = _inputs.Aiming;
        anim.SetTrigger("chargeSun");
        anim.SetBool("isChargingSun", true);

        yield return new WaitForSeconds(_sunCastDelay);

        //ControlFullScreen.instance.ChangeDemond(true);

        _movement.Cast(true);
        ChangeAudio(chargingSun);

        do
        {
            var sun = Instantiate(_sunMagic, _sunSpawnPoint.position, Quaternion.identity);
            sun.player = this;
            sun.SetupStats(_sunBaseDamage);

            float timer = 0;

            while (!_stopChannels && _inputs.SecondaryAttack && !_inputs.launchAttack && timer < _sunMaxChargeTime && CheckAndReduceStamina(_sunHoldCost * Time.deltaTime))
            {
                _rb.angularVelocity = Vector3.zero;

                timer += Time.deltaTime;

                sun.transform.position = _sunSpawnPoint.position;

                sun.UpdateDamage(_sunDamageGrowRate * Time.deltaTime);

                yield return null;
            }

            if (timer >= _sunMaxChargeTime || !CheckAndReduceStamina(_sunHoldCost * Time.deltaTime))
            {
                sun.ChargeFinished();
            }

            while (!_stopChannels && _inputs.SecondaryAttack && !_inputs.launchAttack)
            {
                _rb.angularVelocity = Vector3.zero;

                sun.transform.position = _sunSpawnPoint.position;
                CheckAndReduceStamina(0);
                yield return null;
            }

            if (_stopChannels || !_inputs.SecondaryAttack)
            {
                //sun.transform.SetParent(null);
                sun.StartCoroutine(sun.Cancel());
                //ControlFullScreen.instance.ChangeDemond(false);
            }
            else
            {
                _inputs.launchAttack = false;

                anim.SetTrigger("shootSun");

                timer = 0;

                //yield return new WaitForSeconds(_sunShootDelay);
                while (timer<_sunShootDelay)
                {
                    sun.transform.position = _sunSpawnPoint.position;
                    timer += Time.deltaTime;
                    yield return null;
                }
                //anim.SetBool("IsAttacking", false);

                //sun.transform.SetParent(null);

                Vector3 dir;
                var cameraTransform = _cameraController.AimCamera.transform;

                Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit);
                if (hit.collider)
                {
                    dir = hit.point - sun.transform.position;
                }
                else
                {
                    dir = Camera.main.transform.forward;
                }

                sun.transform.forward = dir;
                sun.Shoot(_sunSpeed);

                //ControlFullScreen.instance.ChangeDemond(false);

                timer = 0;

                while (timer < _sunRecovery && _inputs.SecondaryAttack)
                {
                    timer += Time.deltaTime;

                    yield return null;
                }
            }

            _inputs.launchAttack = false;
        } while (!_stopChannels && _inputs.SecondaryAttack && CheckAndReduceStamina(_sunBaseCost));

        _movement.Cast(false);
        _inputs.ToggleAim(false);
        anim.SetBool("isChargingSun", false);
        //_sunCurrentCooldown = _sunCooldown

        yield return new WaitForSeconds(0.25f);

        _inputs.SecondaryAttack = false;
        _stopChannels = false;
        _aiming = false;
        //_sunCurrentCooldown = _sunCooldown;
    }

    void ActivateObsidianMagic()
    {
        if (Grounded && _obsidianCurrentCooldown <= 0 && CheckAndReduceStamina(_obsidianCost))
        {
            StartCoroutine(UnaimedObsidianMagic());
        }
        else
        {
            _inputs.PrimaryAttack = false;
        }
    }

    void ActivateAimedObsidianMagic()
    {
        if (!_aiming && Grounded && _obsidianCurrentCooldown <= 0)
        {
            StartCoroutine(AimedObsidianMagic());
        }
        else
        {
            _inputs.SecondaryAttack = false;
        }
    }

    IEnumerator UnaimedObsidianMagic()
    {
        _rb.angularVelocity = Vector3.zero;
        _aiming = true;

        yield return new WaitForSeconds(_obsidianShotDelay);

        for (int i = 0; i < _obsidianShardAmount; i++)
        {
            Vector3 dir = transform.forward;

            var shard = Instantiate(_obsidianShard, _obsidianSpawnPoint.position, Quaternion.identity);
            shard.transform.forward = dir;
            shard.SetupStats(_obsidianDamage, _shardSpeed, _shardDuration);

            var xRotation = shard.transform.rotation.eulerAngles.x + Mathf.Lerp(-_shardMaxAngleVariation, _shardMaxAngleVariation, Random.value);
            var yRotation = shard.transform.rotation.eulerAngles.y + Mathf.Lerp(-_shardMaxAngleVariation, _shardMaxAngleVariation, Random.value);
            shard.transform.rotation = Quaternion.Euler(xRotation, yRotation, shard.transform.rotation.eulerAngles.z);
            shard.transform.localScale *= Random.Range(0.9f, 1.1f);
        }

        _obsidianCurrentCooldown = _obsidianCooldown;
        _inputs.SecondaryAttack = false;
        _stopChannels = false;
        _aiming = false;
    }

    IEnumerator AimedObsidianMagic()
    {
        _rb.angularVelocity = Vector3.zero;
        _inputs.ToggleAim(true);
        _aiming = true;
        _inputs.inputUpdate = _inputs.Aiming;

        yield return new WaitForSeconds(_obsidianAimDelay);

        _movement.Cast(true);

        while (!_stopChannels && _inputs.SecondaryAttack)
        {
            _rb.angularVelocity = Vector3.zero;

            if (_inputs.launchAttack && _obsidianCurrentCooldown <= 0 && CheckAndReduceStamina(_obsidianCost))
            {
                yield return new WaitForSeconds(_obsidianShotDelay);

                for (int i = 0; i < _obsidianShardAmount; i++)
                {
                    Vector3 dir;
                    var cameraTransform = _cameraController.AimCamera.transform;

                    Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit);
                    if (hit.collider)
                    {
                        dir = hit.point - _obsidianSpawnPoint.position;
                    }
                    else
                    {
                        dir = Camera.main.transform.forward;
                    }
                    
                    var shard = Instantiate(_obsidianShard, _obsidianSpawnPoint.position, Quaternion.identity);
                    shard.transform.forward = dir;
                    shard.SetupStats(_obsidianDamage, _shardSpeed, _shardDuration);

                    var xRotation = shard.transform.rotation.eulerAngles.x + Mathf.Lerp(-_shardMaxAngleVariation, _shardMaxAngleVariation, Random.value);
                    var yRotation = shard.transform.rotation.eulerAngles.y + Mathf.Lerp(-_shardMaxAngleVariation, _shardMaxAngleVariation, Random.value);
                    shard.transform.rotation = Quaternion.Euler(xRotation, yRotation, shard.transform.rotation.eulerAngles.z);
                    shard.transform.localScale *= Random.Range(0.9f, 1.1f);
                }

                _obsidianCurrentCooldown = _obsidianCooldown;
                _inputs.launchAttack = false;
            }
            else
            {
                _inputs.launchAttack = false;

                yield return null;
            }
        }

        _movement.Cast(false);
        _inputs.ToggleAim(false);

        yield return new WaitForSeconds(0.25f);

        _inputs.SecondaryAttack = false;
        _stopChannels = false;
        _aiming = false;
    }

    void ManageCooldowns()
    {
        if (_stepCurrentCooldown > 0)
        {
            _stepCurrentCooldown -= Time.deltaTime;
        }
        if (_sunCurrentCooldown > 0)
        {
            _sunCurrentCooldown -= Time.deltaTime;
        }
        if (_obsidianCurrentCooldown > 0)
        {
            _obsidianCurrentCooldown -= Time.deltaTime;
        }
        if (_damageCurrentCooldown > 0)
        {
            _damageCurrentCooldown -= Time.deltaTime;
        }
    }

    void StaminaRegeneration()
    {
        if (_stamina < _maxStamina)
        {
            if (_currentStaminaDelay <= 0)
            {
                _stamina += Time.deltaTime * _staminaRegenRate;
                UIManager.instance.UpdateBar(UIManager.Bar.PlayerStamina, _stamina);
            }
            else
            {
                _currentStaminaDelay -= Time.deltaTime;
            }
        }
    }

    bool CheckAndReduceStamina(float cost)
    {
        if (_stamina >= cost)
        {
            _stamina -= cost;
            UIManager.instance.UpdateBar(UIManager.Bar.PlayerStamina, _stamina);
            _currentStaminaDelay = _staminaRegenDelay;
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void TakeDamage(float amount)
    {
        //_inputs.PrimaryAttack = false;
        if (_aiming) _stopChannels = true;

        if (_damageCurrentCooldown > 0) return;
        ChangeAudio(damage);
        _cameraController.CameraShake(1, 0.5f);

        _damageCurrentCooldown = _damageCooldown;

        //anim.SetBool("IsHit", true);
        //anim.SetBool("IsHit", false);

        base.TakeDamage(amount);
        
        UIManager.instance.UpdateBar(UIManager.Bar.PlayerHp, _hp);
    }

    public override void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Joistick()
    {
        _joystickActive = !_joystickActive;
        _inputs.Altern(_joystickActive);
        _inputs.PrimaryAttack = false;
    }

    public void FightStarts(Boss boss)
    {
        //_cameraController.CameraShake(2, 1);
        //SoundManager.instance.ChangeAudioBossFight();
        _movement.Combat(true);
        currentBoss = boss;
    }

    public void FightEnds()
    {
        //_cameraController.CameraShake(2, 1);
        _movement.Combat(false);
        currentBoss = null;
    }

    public void StopCast()
    {
        //anim.SetBool("IsAttacking",false);
        _inputs.PrimaryAttack = false;
    }

    public void RunningAnimation(bool play)
    {
        anim.SetBool("isRunning", play);
        //ChangeAudio(Walking);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            _rb.velocity = Vector3.zero;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
            _inputs.inputUpdate = _inputs.Nothing;
            camaraFinal.SetActive(true);
            _cameraController.Final();
            UIManager.instance.Final();
        }
    }
    public void ChangeAudio(AudioClip clip)
    {
        _myAS.clip = clip;
        _myAS.PlayOneShot(_myAS.clip);
    }
    public IEnumerator PrendoPlayerDust(bool prendo)
    {
        //ForceFieldPlayer.SetActive(prendo);
        if(prendo)
        {
            //dustPlayer.Play();
        }
        yield return new WaitForSeconds(1f);
        //ForceFieldPlayer.SetActive(false);
    }
    public IEnumerator ToggleGameObject(GameObject go, float duration = 1)
    {
        go.SetActive(true);

        yield return new WaitForSeconds(duration);

        go.SetActive(false);
    }
}
