using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
using UnityEngine.Audio;

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

    public Transform target;

    [SerializeField] float _maxStamina, _staminaRegenRate, _staminaRegenDelay, _damageCooldown, _speed, _explorationSpeed, _speedOnCast, _turnRate, _jumpStr, _stepStr, _castStepStr, _stepCooldown/*(variable del step viejo)_stepStopVelocity*/;
    [SerializeField] LayerMask _floorLayer;
    [SerializeField] Transform _aimTarget;
    [SerializeField] Transform[] _feet;
    [SerializeField] InteractionManager _interaction;
    [SerializeField] VisualEffect[] _handFires;
    [SerializeField] AudioMixer _audioMixer;

    [Header("Stamina costs")]
    [SerializeField] float _jumpCost;
    [SerializeField] float _stepCost, _sunBaseCost, _sunHoldCost, _obsidianCost;

    [Header("Roll")]
    [SerializeField] float _invincibilityTime;
    [SerializeField] float _postRollTurnRate, _postRollMoveRecoveryDuration;

    [Header("Sun Magic")]
    [SerializeField] SunMagic _sunMagic;
    [SerializeField] SunMagic _finisher;
    [SerializeField] SunBasic _magicTest;
    [SerializeField] LayerMask _raycastTargets;
    public Transform[] sunSpawnPoint;
    [SerializeField] int _attacksToFinisher;
    [SerializeField] float _comboBreakTime, _sunBaseDamage, _sunDamageGrowRate, _sunSpeed, _sunMaxChargeTime, _sunCastDelay, _sunShootDelay, _sunRecovery, _sunCooldown, _sunAbsorbTime, _sunMeleeDuration, _sunHitboxX, _sunHitboxY, _sunHitboxZ, _sunRange;
    Vector3 _sunHitbox;

    ObjectPool<SunMagic> _sunPool, _finisherPool;
    Factory<SunMagic> _sunFactory, _finisherFactory;

    [Header("Obsidian Magic")]
    [SerializeField] PlayerProjectile _obsidianShard;
    [SerializeField] Transform _obsidianSpawnPoint;
    [SerializeField] float _obsidianDamage, _obsidianAimDelay, _obsidianShotDelay, _obsidianComboInterval, _obsidianCooldown, _shardMaxAngleVariation, _shardSpeed, _shardDuration;
    [SerializeField] int _obsidianShardAmount, _maxWaves;

    float _stepCurrentCooldown = 0, _obsidianCurrentCooldown = 0, _sunCurrentCooldown = 0, _damageCurrentCooldown = 0;

    float _stamina, _currentStaminaDelay = 0, _airTime = 0;

    [HideInInspector] public Boss currentBoss;

    MagicType _activeMagic;

    [SerializeField] CinemachineCameraController _cameraController;

    public GameObject camaraFinal;

    [HideInInspector] public bool canAttack = true;
    private bool _joystickActive = true, _aiming = false, _stopChannels = false, _comboing, _canChain = false, _dead = false, _invincible = false;

    public bool Dead { get { return _dead; } }

    [SerializeField] Material _VignetteAmountClamps;

    [SerializeField] ParticleSystem dustPlayer;
    [SerializeField] GameObject ForceFieldPlayer;

    SpecialsManager _specials;
    
    public SpecialsManager Specials
    {
        get
        {
            return _specials;
        }
    }

    [SerializeField] Animation myFireAnim;

    public Trigger OnHit;

    public bool StopChannels
    {
        get
        {
            return _stopChannels;
        }
        set
        {
            _stopChannels = value;
        }
    }

    public bool Aiming
    {
        get
        {
            return _comboing;
        }
    }

    bool _grounded = true;

    public bool Grounded
    {
        get { return _grounded; }

        set
        {
            if (value == true && _grounded == false && _airTime >= 0.1f) Land();

            _grounded = value;
        }
    }

    public float DistToFloor
    {
        get
        {
            return _movement.DistanceToFloor();
        }
    }

    public Vector3 LookDir = Vector3.zero;

    public Animator anim;

    AudioSource _audioSource;

    Coroutine _postStepCoroutine;

    CapsuleCollider _collider;

    float _colliderHeight, _colliderCenterY;

    public enum MagicType
    {
        Sun,
        Obsidian
    }

    protected override void Awake()
    {
        base.Awake();
        _collider = GetComponent<CapsuleCollider>();
        _audioSource = GetComponent<AudioSource>();
        _movement = new Movement(transform, _rb, _speed, _explorationSpeed, _speedOnCast, _turnRate, _jumpStr, _stepStr, _castStepStr, _floorLayer);
        _inputs = new Inputs(_movement, this/*, _cameraController*/);
        _specials = GetComponent<SpecialsManager>();
        OnHit = new Trigger();

        _activeMagic = MagicType.Sun;
        _sunHitbox = new Vector3(_sunHitboxX, _sunHitboxY, _sunHitboxZ);
        _inputs.inputUpdate = _inputs.Nothing;

        _colliderHeight = _collider.height;
        _colliderCenterY = _collider.center.y;
    }

    void Start()
    {
        _sunFactory = new Factory<SunMagic>(_sunMagic);
        _sunPool = new ObjectPool<SunMagic>(_sunFactory.GetObject, SunMagic.TurnOff, SunMagic.TurnOn, 10);

        _finisherFactory = new Factory<SunMagic>(_finisher);
        _finisherPool = new ObjectPool<SunMagic>(_finisherFactory.GetObject, SunMagic.TurnOff, SunMagic.TurnOn, 2);

        UIManager.instance.UpdateBar(UIManager.Bar.PlayerHp, _hp, _maxHp);

        _stamina = _maxStamina;
        UIManager.instance.UpdateBar(UIManager.Bar.PlayerStamina, _stamina, _maxStamina);

        Joistick();

        if (GameManager.instance.playerWorldPos != Vector3.zero) transform.position = GameManager.instance.playerWorldPos;
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
        Grounded = _movement.IsGrounded();

        if (!Grounded) _airTime += Time.fixedDeltaTime;
        else _airTime = 0;

        if (LookDir != Vector3.zero)
        {
            if (_movement.Rotate(LookDir)) LookDir = Vector3.zero;
        }

        _inputs.InputsFixedUpdate();

        if (_comboing) LookDir = Camera.main.transform.forward.MakeHorizontal();
    }

    private void LateUpdate()
    {
        _inputs.InputsLateUpdate();
    }

    public void GetUp()
    {
        _inputs.inputUpdate = _inputs.JustCamera;

        anim.SetTrigger("WakeUp");
    }

    public void GotUp()
    {
        if (GameManager.instance.Json.saveData.finishedTutorial) _inputs.inputUpdate = _inputs.Unpaused;
        else
        {
            _inputs.inputUpdate = _inputs.NoAttacks;

            GameManager.instance.tutorial.GotUp();
        }
    }

    public void Invincibility(int value)
    {
        if (value == 1)
        {
            _audioSource.PlayOneShot(AudioManager.instance.roll);

            _invincible = true;
        }
        else _invincible = false;
    }

    public void RollStarted()
    {
        _inputs.inputUpdate = _inputs.FixedCast;

        _collider.center = new Vector3(0, _colliderCenterY * 0.6f);
        _collider.height = _colliderHeight * 0.6f;
    }

    public void RollEnded()
    {
        if (_dead) return;

        _inputs.inputUpdate = _inputs.Unpaused;

        _postStepCoroutine = StartCoroutine(_comboing ? _movement.OnRollEnd(_postRollTurnRate, _turnRate, _postRollMoveRecoveryDuration) : _movement.OnRollEnd(_postRollTurnRate, _turnRate, 0, _speed, _postRollMoveRecoveryDuration));

        _collider.center = new Vector3(0, _colliderCenterY);
        _collider.height = _colliderHeight;
    }

    public void Cutscene(bool starts)
    {
        if (starts)
        {
            Inputs.inputUpdate = Inputs.Nothing;
            _rb.isKinematic = true;
        }
        else
        {
            Inputs.inputUpdate = Inputs.Unpaused;
            _rb.isKinematic = false;
        }
    }

    public void Jump()
    {
        if (Grounded && _stepCurrentCooldown <= _stepCooldown * 0.5f && CheckAndReduceStamina(_jumpCost))
        {
            //StartCoroutine(ToggleGameObject(_jumpParticles));
            anim.SetTrigger("jump");
            _movement.Jump();
            _audioSource.PlayOneShot(AudioManager.instance.jump);
            //anim.SetBool("IsJumping", false);
        }
    }

    public void Land()
    {
        Physics.Raycast(transform.position, Vector3.down, out var hit, _floorLayer);

        _audioSource.PlayOneShot(AudioManager.instance.PlayerLanding(hit.collider != null ? hit.collider.gameObject.tag : ""));
    }

    public void Roll(float horizontalInput, float verticalInput)
    {
        if (Grounded && _stepCurrentCooldown <= 0 && CheckAndReduceStamina(_stepCost))
        {
            if (_comboing) _stopChannels = true;
            anim.SetTrigger("roll");
            //StartCoroutine(ToggleGameObject(_stepParticles));
            //anim.SetBool("IsStrafeRight", true);
            _stepCurrentCooldown = _stepCooldown;
            _movement.Roll(horizontalInput, verticalInput);
        }
    }

    public void UseSpecial(int slot)
    {
        if (Grounded)
        {
            if(_specials.IsOffCooldown(slot))
            {
                var cost = _specials.GetCost(slot);

                if (CheckStamina(cost))
                {
                    if (_specials.ActivateSpecial(slot))
                    {
                        ReduceStamina(cost);
                    }
                }
            }
            else
            {
                UIManager.instance.SpecialUnavailable(slot);
            }
        }
            
    }

    public void ChangeActiveMagic(MagicType type)
    {
        _activeMagic = type;
        //UIManager.instance.UpdateBasicSpell(type);
    }

    public void ActivateMagic()
    {
        switch (_activeMagic)
        {
            case MagicType.Sun:
                if (!_comboing) ActivateSunMagic();
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
        if (_movement.IsGrounded() && _sunCurrentCooldown <= 0 && CheckStamina(_sunBaseCost) && _damageCurrentCooldown <= 0 && canAttack)
        {
            StartCoroutine(RootMotionCombo());
        }
        else
        {
            _inputs.PrimaryAttack = false;
            //_inputs.SecondaryAttack = false;
        }
    }

    void ActivateAimedSunMagic()
    {
        if (!_aiming && _movement.IsGrounded() && _sunCurrentCooldown <= 0/* && CheckAndReduceStamina(_sunBaseCost)*/)
        {
            //StartCoroutine(BasicAimedCombo());
        }
        else
        {
            //_inputs.PrimaryAttack = false;
            _inputs.SecondaryAttack = false;
        }
    }

    IEnumerator NewAimedSunMagic()
    {
        _rb.angularVelocity = Vector3.zero;
        _inputs.ToggleAim(true);
        _aiming = true;
        _inputs.inputUpdate = _inputs.Aiming;
        anim.SetTrigger("chargeSun");
        anim.SetBool("isChargingSun", true);

        yield return new WaitForSeconds(_sunCastDelay);

        _movement.Cast(true);
        //ChangeAudio(chargingSun);

        var sun = Instantiate(_magicTest, sunSpawnPoint[0].position, Quaternion.identity);
        sun.SetupStats(_sunBaseDamage, 0, 0.4f);

        while (!_stopChannels && _inputs.SecondaryAttack)
        {
            _rb.angularVelocity = Vector3.zero;

            if (sun != null)
            {
                sun.transform.position = sunSpawnPoint[0].position;

                if (_inputs.launchAttack)
                {
                    if (CheckAndReduceStamina(_sunBaseCost))
                    {
                        anim.SetTrigger("shootSun");

                        float timer = 0;

                        while (timer < _sunShootDelay)
                        {
                            sun.transform.position = sunSpawnPoint[0].position;
                            timer += Time.deltaTime;
                            yield return null;
                        }

                        _sunCurrentCooldown = _sunCooldown;

                        sun.Launch((_cameraController.AimCamera.transform.forward + Vector3.up * 0.2f).normalized, 400);
                        sun = null;
                    }

                    _inputs.launchAttack = false;
                }
            }
            else
            {
                if (_sunCurrentCooldown <= 0)
                {
                    sun = Instantiate(_magicTest, sunSpawnPoint[0].position, Quaternion.identity);
                    sun.SetupStats(_sunBaseDamage, 0, 0.4f);
                }
            }

            yield return null;
        }

        if (sun != null)
        {
            sun.Die();
        }

        _movement.Cast(false);
        _inputs.ToggleAim(false);
        anim.SetBool("isChargingSun", false);

        yield return new WaitForSeconds(0.25f);

        _inputs.launchAttack = false;
        _inputs.SecondaryAttack = false;
        _stopChannels = false;
        _aiming = false;
    }

    IEnumerator BasicCombo()
    {
        _rb.angularVelocity = Vector3.zero;
        anim.SetBool("isChargingSun", true);
        _comboing = true;

        _movement.Cast(true);

        float halfCD = _sunCooldown * 0.5f, currentComboTime;
        int comboCount = 1;

        currentComboTime = _comboBreakTime;

        _sunCurrentCooldown = _sunCooldown;

        while (!_stopChannels && currentComboTime > 0)
        {
            _rb.angularVelocity = Vector3.zero;

            currentComboTime -= Time.deltaTime;

            if (_inputs.PrimaryAttack)
            {
                if (_sunCurrentCooldown <= 0 && CheckAndReduceStamina(_sunBaseCost))
                {
                    if (comboCount >= _attacksToFinisher)
                    {
                        comboCount = 0;
                        currentComboTime = _comboBreakTime * 0.75f;

                        anim.SetTrigger("progressCombo");

                        _sunCurrentCooldown = _sunCooldown * 1.25f;
                    }
                    else
                    {
                        comboCount++;
                        currentComboTime = _comboBreakTime;

                        anim.SetTrigger("progressCombo");

                        _sunCurrentCooldown = _sunCooldown;
                    }
                }
                else if (_sunCurrentCooldown > halfCD)
                {
                    _inputs.PrimaryAttack = false;

                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
        }

        _movement.Cast(false);
        anim.SetBool("isChargingSun", false);

        yield return new WaitForSeconds(0.25f);

        _inputs.PrimaryAttack = false;
        _stopChannels = false;
        _comboing = false;
    }

    IEnumerator RootMotionCombo()
    {
        foreach (var item in _handFires)
        {
            item.Play();
        }

        _rb.angularVelocity = Vector3.zero;

        _comboing = true;
        
        anim.SetBool("isComboing", true);

        if (_inputs.HorizontalInput < -0.25f) anim.SetTrigger("leftCombo");
        else if (_inputs.HorizontalInput > 0.25f) anim.SetTrigger("rightCombo");
        else anim.SetTrigger("standingCombo");

        if (_postStepCoroutine != null) StopCoroutine(_postStepCoroutine);
        _movement.FixedCast(true);

        float currentComboTime;
        int comboCount = 1;

        currentComboTime = _comboBreakTime;

        _inputs.PrimaryAttack = false;
        _canChain = false;

        while (!_stopChannels && currentComboTime > 0)
        {
            _rb.angularVelocity = Vector3.zero;

            currentComboTime -= Time.deltaTime;

            if (_inputs.SecondaryAttack && comboCount >= _attacksToFinisher)
            {
                if (_canChain)
                {
                    if (CheckStamina(_sunBaseCost))
                    {
                        comboCount = 0;
                        currentComboTime = _comboBreakTime;

                        anim.SetTrigger("comboFinisher");

                        _inputs.SecondaryAttack = false;
                        _canChain = false;

                        yield return null;
                    }
                    else
                    {
                        yield return null;
                    }
                }
                else
                {
                    _inputs.SecondaryAttack = false;

                    yield return null;
                }
            }
            else if (_inputs.PrimaryAttack)
            {
                if (_canChain)
                {
                    if (CheckStamina(_sunBaseCost))
                    {
                        comboCount++;
                        currentComboTime = _comboBreakTime;

                        if (_inputs.HorizontalInput < -0.25f) anim.SetTrigger("leftCombo"); // combo a la izquierda
                        else if (_inputs.HorizontalInput > 0.25f) anim.SetTrigger("rightCombo"); // combo a la derecha
                        else anim.SetTrigger("standingCombo");

                        _inputs.PrimaryAttack = false;
                        _canChain = false;

                        yield return null;
                    }
                    else
                    {
                        yield return null;
                    }
                }
                else
                {
                    _inputs.PrimaryAttack = false;
                    _inputs.SecondaryAttack = false;

                    yield return null;
                }
            }
            else
            {
                _inputs.SecondaryAttack = false;
                yield return null;
            }
        }
        
        _movement.FixedCast(false);
        anim.SetBool("isComboing", false);
        _comboing = false;

        foreach (var item in _handFires)
        {
            item.Stop();
        }

        yield return new WaitForSeconds(0.25f);

        _inputs.PrimaryAttack = false;
        _inputs.SecondaryAttack = false;
        _stopChannels = false;
        _canChain = false;
    }

    public void ThrowFireball(int handIndex)
    {
        ReduceStamina(_sunBaseCost);

        var sun = _sunPool.Get();
        sun.transform.position = sunSpawnPoint[handIndex].position;

        Vector3 dir;
        var cameraTransform = Camera.main.transform;

        Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, Mathf.Infinity, _raycastTargets);

        if (hit.collider)
        {
            dir = hit.point - sun.transform.position;
        }
        else
        {
            dir = cameraTransform.forward;
        }

        sun.transform.forward = dir;

        sun.Initialize(_sunPool, _sunBaseDamage, _sunSpeed);

        _canChain = true;

        _audioMixer.SetFloat("Pitch", Random.Range(0.95f, 1.05f));
        _audioSource.PlayOneShot(AudioManager.instance.PlayerCombo());
    }

    public void ThrowEnhancedFireball(int handIndex)
    {
        ReduceStamina(_sunBaseCost);

        var sun = _finisherPool.Get();
        sun.transform.position = sunSpawnPoint[handIndex].position;

        Vector3 dir;
        var cameraTransform = Camera.main.transform;

        Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, Mathf.Infinity, _raycastTargets);

        if (hit.collider)
        {
            dir = hit.point - sun.transform.position;
        }
        else
        {
            dir = cameraTransform.forward;
        }

        sun.transform.forward = dir;

        sun.Initialize(_finisherPool, _sunBaseDamage * 2f, _sunSpeed);

        _canChain = true;

        _audioMixer.SetFloat("Pitch", Random.Range(0.95f, 1.05f));
        _audioSource.PlayOneShot(AudioManager.instance.PlayerCombo());
    }

    IEnumerator BasicAimedCombo()
    {
        _rb.angularVelocity = Vector3.zero;
        _inputs.ToggleAim(true);
        _aiming = true;
        _inputs.inputUpdate = _inputs.Aiming;
        anim.SetTrigger("chargeSun");
        anim.SetBool("isChargingSun", true);

        yield return new WaitForSeconds(_sunCastDelay);

        _movement.Cast(true);

        float halfCD = _sunCooldown * 0.5f, currentComboTime = 0;
        int comboCount = 0;

        while (!_stopChannels && _inputs.SecondaryAttack)
        {
            _rb.angularVelocity = Vector3.zero;

            currentComboTime -= Time.deltaTime;

            if (currentComboTime <= 0) comboCount = 0;

            if (_inputs.launchAttack)
            {
                if (_sunCurrentCooldown <= 0 && CheckAndReduceStamina(_sunBaseCost))
                {
                    if (comboCount >= _attacksToFinisher)
                    {
                        comboCount = 0;

                        anim.SetTrigger("shootSun");

                        yield return new WaitForSeconds(_sunShootDelay);

                        _sunCurrentCooldown = _sunCooldown;

                        var sun = Instantiate(_sunMagic, sunSpawnPoint[0].position, Quaternion.identity);
                        sun.transform.localScale *= 4;
                        sun.SetupStats(_sunBaseDamage * 1.5f);

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

                        sun.Shoot(_sunSpeed * 1.5f);
                    }
                    else
                    {
                        comboCount++;
                        currentComboTime = _comboBreakTime;

                        anim.SetTrigger("shootSun");

                        yield return new WaitForSeconds(_sunShootDelay);

                        _sunCurrentCooldown = _sunCooldown;

                        var sun = Instantiate(_sunMagic, sunSpawnPoint[0].position, Quaternion.identity);
                        sun.SetupStats(_sunBaseDamage);
                        sun.ChargeFinished();

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
                    }
                }
                else if (_sunCurrentCooldown > halfCD)
                {
                    _inputs.launchAttack = false;

                    yield return null;
                }
                else
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
        }

        _movement.Cast(false);
        _inputs.ToggleAim(false);
        anim.SetBool("isChargingSun", false);

        yield return new WaitForSeconds(0.25f);

        _inputs.launchAttack = false;
        _inputs.SecondaryAttack = false;
        _stopChannels = false;
        _aiming = false;
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
            StartCoroutine(NewAimedSunMagic());
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

    bool CheckStamina(float cost)
    {
        if (_stamina >= cost)
        {
            return true;
        }
        else
        {
            UIManager.instance.NotEnoughStamina();
            return false;
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
            UIManager.instance.NotEnoughStamina();
            return false;
        }
    }

    public void ReduceStamina(float cost)
    {
        _stamina -= cost;
        UIManager.instance.UpdateBar(UIManager.Bar.PlayerStamina, _stamina);
        _currentStaminaDelay = _staminaRegenDelay;
    }

    public void Interact()
    {
        _interaction.StartInteraction();
    }
    
    public override void TakeDamage(float amount, bool bypassCooldown = false)
    {
        if (_invincible || _damageCurrentCooldown > 0 || _dead) return;

        //_inputs.PrimaryAttack = false;
        OnHit.Triggers(this);
        if (_comboing) _stopChannels = true;

        //ChangeAudio(damage);
        //_cameraController.CameraShake(1, 0.5f);
        UIManager.instance.TookDamage();
        if (!bypassCooldown) _damageCurrentCooldown = _damageCooldown;

        anim.SetTrigger("tookDamage");

        base.TakeDamage(amount);
        
        UIManager.instance.UpdateBar(UIManager.Bar.PlayerHp, _hp);

        //if (_hp <= _maxHp * 0.3f)
        //{
        //    UIManager.instance.LowHp();
        //}
    }

    public override void Die()
    {
        _dead = true;

        anim.SetLayerWeight(4, 0);

        StartCoroutine(Death());
    }

    IEnumerator Death()
    {
        _inputs.inputUpdate = _inputs.Nothing;

        anim.SetTrigger("die");

        yield return new WaitForSeconds(5);

        SceneLoader.instance.LoadLevel(SceneManager.GetActiveScene().buildIndex);
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

    public void UncapResources(bool uncap)
    {
        _maxStamina = uncap ? 10000 : 100;
        _maxHp = uncap ? 10000 : 100;
        _stamina = _maxStamina;
        _hp = _maxHp;
        UIManager.instance.UpdateBar(UIManager.Bar.PlayerStamina, _stamina, _maxStamina);
        UIManager.instance.UpdateBar(UIManager.Bar.PlayerHp, _hp, _maxHp);
    }

    public void TravelToPalace()
    {
        myFireAnim.Play();
        anim.SetTrigger("pray");
    }

    public void Footstep(int footIndex)
    {
        Physics.Raycast(_feet[footIndex].transform.position, Vector3.down, out var hit, _floorLayer);

        _audioMixer.SetFloat("Pitch", Random.Range(0.95f, 1.05f));
        
        _audioSource.PlayOneShot(AudioManager.instance.PlayerFootstep(hit.collider != null ? hit.collider.gameObject.tag : ""));
    }
}
