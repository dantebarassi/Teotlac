using IA2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.VFX;

public class NewItztlacoliuhqui : Boss
{
    public enum Actions
    {
        Inactive,
        Approach,
        Sidestep,
        Melee,
        QuickShot,
        PowerShot,
        GroundSpike,
        BudStrike,
        Bloom,
        GiantKnife,
        Limb,
        Garden
    }

    EventFSM<Actions> _fsm;
    Pathfinding _pf;

    [SerializeField] bool _playOnStart;
    [SerializeField] PlayerController _player;
    [SerializeField] Transform _losTransform;
    [SerializeField] Transform[] _footPos;
    [SerializeField] LayerMask _playerLayer, _groundLayer, _obstacleLayer;
    [SerializeField] float _turnRate, _aggroRange;
    [SerializeField] GameObject _deathCam;

    [Header("Chain attacks")]
    [SerializeField] Actions[] _chainableAttacks;
    [Range(0, 100)]
    [SerializeField] int _baseChainChance, _chainChanceReduction;
    int _chainCounter = 0;

    [Header("Basic Shards")]
    [SerializeField] ObsidianShard _basicShardPrefab;
    [SerializeField] float _basicShardSpeed, _basicShardDamage;

    [Header("Homing Shards")]
    [SerializeField] ObsidianShard _homingShardPrefab;
    [SerializeField] float _homingShardSpeed, _homingShardDamage;

    [Header("Walk")]
    [SerializeField] float _walkSpeed;
    [SerializeField] float _minWalkDuration, _maxWalkDuration, _walkReactionCheckCD;
    [Range(0, 100)]
    [SerializeField] int _walkActionChance, _walkReactionChance;

    [Header("Melee")]
    [SerializeField] VisualEffect _melee;
    [SerializeField] float _meleeRange, _meleeSpawnOffset, _meleeRadius, _meleeHeight, _meleeDamage;

    [Header("Buds")]
    [SerializeField] ObsidianBud _budPrefab;
    [SerializeField] int _projectileAmount;
    [SerializeField] float _budActivationRange, _budStrikeDamage;

    [Header("Quick Shot")]
    [SerializeField] Transform _quickShotSpawnPos;
    [SerializeField] int _quickShotCount, _enhancedQuickShotCount;
    [SerializeField] float _quickShotHorDirVariation, _quickShotVerDirVariation;

    [Header("Power Shot")]
    [SerializeField] Transform _powerShotSpawnPos;
    [SerializeField] int _powerShotCount, _enhancedPowerShotCount;
    [SerializeField] float _powerShotSpawnOffset;

    List<ObsidianShard> _homingShards = new();

    [Header("Ground Spikes")]
    [SerializeField] ObsidianGroundSpikes _spikesPrefab;
    [SerializeField] float _spikesStartOffset, _spikesOffset, _spikesBaseSizeX, _spikesSizeY, _spikesSizeZ, _spikesSizeGrowthX, _spikesDelay, _spikesDamage, _spikesDuration, spikeDespawnTime, _spikesSpeed;
    [SerializeField] int _spikesExtraWaves, _spikesMaxRows, _spikeCount, _totalAngle;
    [SerializeField] LayerMask _spikeTargets;

    [Header("Obsidian Limb")]
    [SerializeField] Vector3 _limbCheckBoxSize;
    [SerializeField] GameObject _limb, _limbExplosion;
    [SerializeField] float _limbWindUpDuration, _enhancedLimbWindUpDuration, _limbDamage, _limbRange;
    float _currentLimbWindUp;

    [Header("Giant Knife")]
    [SerializeField] GiantObsidianKnife _gKnife;
    [SerializeField] float _gKnifeThrustDamage, _gKnifeSliceDamage, _gKnifeRange;

    [Header("Placeholder Wall Spike")]
    [SerializeField] ObsidianWall _wallPrefab;
    [SerializeField] VisualEffect _movingSpikes,_obelisc,circles1, circles2, circles3, circles4;
    [SerializeField] float _wallSpikeSpawnOffset, _wallSpikeTravelSpeed, _wallSpawnDelay, _wallSpikeKnockback, _wallSpikeDamage;

    ObjectPool<ObsidianBud> _budPool;
    Factory<ObsidianBud> _budFactory;
    ObjectPool<ObsidianShard> _basicShardPool;
    Factory<ObsidianShard> _basicShardFactory;
    ObjectPool<ObsidianShard> _homingShardPool;
    Factory<ObsidianShard> _homingShardFactory;
    ObjectPool<ObsidianGroundSpikes> _spikesPool;
    Factory<ObsidianGroundSpikes> _spikesFactory;

    List<ObsidianBud> _spawnedBuds = new();
    List<ObsidianBud> _bloomingBuds = new();

    AudioSource _myAS;
    Animator _anim;

    Vector3 _lookDir = Vector3.zero;

    Queue<Actions> _actionHistory = new();

    float _timer = 0, _timer2 = 0;
    bool _start = false, _stopGroundSpikes = false, _activated = false, _trackPlayer = false, _secondPhase = false;

    Material _bodyMaterial;
    [SerializeField] GameObject _bodyObj;

    public void TrackPlayer(int value)
    {
        if (value == 0) _trackPlayer = false;
        else _trackPlayer = true;
    }

    protected override void Awake()
    {
        base.Awake();

        //_spikesPrefab.SetUpVFX(_spikesOffset, _spikesDuration);

        _anim = GetComponent<Animator>();

        _budFactory = new Factory<ObsidianBud>(_budPrefab);
        _budPool = new ObjectPool<ObsidianBud>(_budFactory.GetObject, ObsidianBud.TurnOff, ObsidianBud.TurnOn, 10);

        _basicShardFactory = new Factory<ObsidianShard>(_basicShardPrefab);
        _basicShardPool = new ObjectPool<ObsidianShard>(_basicShardFactory.GetObject, ObsidianShard.TurnOff, ObsidianShard.TurnOn, 15);

        _homingShardFactory = new Factory<ObsidianShard>(_homingShardPrefab);
        _homingShardPool = new ObjectPool<ObsidianShard>(_homingShardFactory.GetObject, ObsidianShard.TurnOff, ObsidianShard.TurnOn, 15);

        _spikesFactory = new Factory<ObsidianGroundSpikes>(_spikesPrefab);
        _spikesPool = new ObjectPool<ObsidianGroundSpikes>(_spikesFactory.GetObject, ObsidianGroundSpikes.TurnOff, ObsidianGroundSpikes.TurnOn, 200);

        _gKnife.SetUp(_gKnifeThrustDamage, _gKnifeSliceDamage);

        _actionHistory.Enqueue(Actions.Inactive);
        _actionHistory.Enqueue(Actions.Inactive);
        //_actionHistory.Enqueue(Actions.Inactive);

        _currentLimbWindUp = _limbWindUpDuration;
        _bodyMaterial = _bodyObj.GetComponent<Renderer>().material;

        if (_playOnStart) StartCoroutine(SetupWait());
    }

    IEnumerator SetupWait()
    {
        yield return new WaitForSeconds(1);

        Setup();
    }

    public void Setup()
    {
        _rb = GetComponent<Rigidbody>();
        _myAS = GetComponent<AudioSource>();
        _pf = new Pathfinding();

        #region FSM State Creation

        var inactive = new State<Actions>("Inactive");
        var approach = new State<Actions>("Approach");
        var sidestep = new State<Actions>("Sidestep");
        var melee = new State<Actions>("Melee");
        var quickShot = new State<Actions>("QuickShot");
        var powerShot = new State<Actions>("PowerShot");
        var groundSpike = new State<Actions>("GroundSpike");
        var budStrike = new State<Actions>("BudStrike");
        var bloom = new State<Actions>("Bloom");
        var giantKnife = new State<Actions>("GiantKnife");
        var limb = new State<Actions>("Limb");
        var garden = new State<Actions>("Garden");

        StateConfigurer.Create(inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(approach)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(sidestep)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(melee)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(quickShot)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(powerShot)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(groundSpike)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .Done();

        StateConfigurer.Create(budStrike)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(bloom)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(giantKnife)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(limb)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(garden)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Sidestep, sidestep)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.GroundSpike, groundSpike)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.GiantKnife, giantKnife)
            .SetTransition(Actions.Limb, limb)
            .Done();

        #endregion

        #region FSM State Setup

        inactive.OnUpdate += () =>
        {
            if (_player.Dead) return;

            if (Vector3.Distance(transform.position, _player.transform.position) <= _aggroRange)
            {
                UIManager.instance.UpdateBar(UIManager.Bar.BossHp, _hp, _maxHp);
                UIManager.instance.ChangeBossName("Itztlacoliuhqui, God of Obsidian");
                _player.FightStarts(this);
                UIManager.instance.ToggleBossBar(true);
                ChangeState(Actions.Approach);
            }
        };

        approach.OnEnter += x =>
        {
            _anim.SetBool("isWalking", true);

            _trackPlayer = true;

            _timer = 0;
            _timer2 = 0;
        };

        approach.OnUpdate += () =>
        {
            if (_timer2 > 0) _timer2 -= Time.deltaTime;
        };

        approach.OnFixedUpdate += () =>
        {
            Debug.Log("approaching");

            _timer += Time.fixedDeltaTime;

            _rb.MovePosition(transform.position + transform.forward * _walkSpeed * Time.fixedDeltaTime);

            if (CanMelee() || Physics.Raycast(_losTransform.position, transform.forward, _meleeRange, _obstacleLayer))
            {
                ChangeState(Actions.Melee);
            }
            else if (_timer % _minWalkDuration < 0.05f)
            {
                if (Random.Range(0, 100) < _walkActionChance)
                {
                    ChainOpportunity();
                }
            }
            else if (_player.Aiming && _timer2 <= 0)
            {
                if (Random.Range(0, 100) < _walkReactionChance)
                {
                    ChangeState(Actions.Sidestep);
                    // o directamente poner la animacion de quickshot 
                }
                else
                {
                    _timer2 = _walkReactionCheckCD;
                }
            }
            else if (_timer > _maxWalkDuration)
            {
                ChainOpportunity();
            }
        };

        approach.OnExit += x =>
        {
            _trackPlayer = false;

            _anim.SetBool("isWalking", false);
        };

        sidestep.OnEnter += x =>
        {
            _anim.SetTrigger("sidestep");

            _trackPlayer = true;
        };

        melee.OnEnter += x =>
        {
            _anim.SetTrigger("Melee");
        };

        quickShot.OnEnter += x =>
        {
            _anim.SetTrigger("Attack");
        };

        powerShot.OnEnter += x =>
        {
            _anim.SetTrigger("PowerShot");
        };

        groundSpike.OnEnter += x =>
        {
            _anim.SetTrigger(_secondPhase ? "DoubleStomp" : "Stomp");

            _trackPlayer = true;
        };

        bloom.OnEnter += x =>
        {
            StartCoroutine(BloomTest());
        };

        giantKnife.OnEnter += x =>
        {
            _trackPlayer = true;

            _anim.SetTrigger("Thrust");
        };

        limb.OnEnter += x =>
        {
            _timer2 = 0;
            _activated = false;

            _limbExplosion.transform.position = _limb.transform.position;
            _limbExplosion.transform.parent = _limb.transform;

            _anim.SetTrigger("AttackLimb");
        };

        limb.OnUpdate += () =>
        {
            if (!_activated)
            {
                _timer2 += Time.deltaTime;

                if (_timer2 >= _currentLimbWindUp)
                {
                    _activated = true;
                    _anim.SetBool("ChargingLimb", true);
                    _anim.SetTrigger("AttackLimb");
                }
            }
        };

        limb.OnExit += x =>
        {
            _timer2 = 0;
            _activated = false;
            _anim.SetBool("ChargingLimb", false);
        };

        _fsm = new EventFSM<Actions>(inactive);

        #endregion

        _start = true;
    }

    void ChangeState(Actions action) => _fsm.SendInput(action);

    bool CanMelee() => Vector3.Distance(transform.position, _player.transform.position) <= _meleeRange;
    bool CanQuickShot() => _losTransform.position.InLineOfSightOf(_player.target.position, _obstacleLayer);
    bool CanGroundSpike() => true;
    bool CanBloom()
    {
        _bloomingBuds = _spawnedBuds.Where(x => Vector3.Distance(_player.transform.position, x.transform.position) <= _budActivationRange).ToList();

        return _bloomingBuds.Any();
    }
    bool CanLimb() => Vector3.Distance(transform.position, _player.transform.position) <= _limbRange;
    bool CanHoming() => true;
    bool CanGiantKnife() => Vector3.Distance(transform.position, _player.transform.position) <= _gKnifeRange;

    bool TryChain(Actions action)
    {
        bool succesful = false;

        switch (action)
        {
            case Actions.QuickShot:
                if (CanQuickShot()) succesful = true;
                break;
            case Actions.GroundSpike:
                if (CanGroundSpike()) succesful = true;
                break;
            case Actions.Limb:
                if (CanLimb()) succesful = true;
                break;
            case Actions.PowerShot:
                if (CanHoming()) succesful = true;
                break;
            case Actions.GiantKnife:
                if (CanGiantKnife()) succesful = true;
                break;
            default:
                break;
        }

        if (succesful) ChangeState(action);

        return succesful;
    }

    void Update()
    {
        if (!_start) return;

        _fsm.Update();
    }

    private void FixedUpdate()
    {
        if (!_start) return;

        if (_trackPlayer) _lookDir = (_player.transform.position - transform.position).MakeHorizontal();

        if (_lookDir != Vector3.zero)
        {
            if (Rotate(_lookDir)) _lookDir = Vector3.zero;
        }

        _fsm.FixedUpdate();
    }

    public bool Rotate(Vector3 dir)
    {
        var eulerRotation = transform.rotation.eulerAngles;

        var yRotation = Vector3.Angle(transform.right, dir) < Vector3.Angle(-transform.right, dir) ? _turnRate : -_turnRate;

        var angleToDesired = Vector3.Angle(transform.forward, dir);

        yRotation *= Time.fixedDeltaTime;
        var absYRotation = Mathf.Abs(yRotation);

        if (angleToDesired > absYRotation)
        {
            _rb.MoveRotation(Quaternion.Euler(eulerRotation.x, eulerRotation.y + yRotation, eulerRotation.z));

            return false;
        }
        else
        {
            transform.forward = dir;

            return true;
        }
    }

    IEnumerator MeleeTest()
    {
        yield return new WaitForSeconds(0.5f);

        Melee();

        yield return new WaitForSeconds(1.75f);

        EndChain();
    }

    IEnumerator BloomTest()
    {
        Prebloom();

        yield return new WaitForSeconds(1);

        Bloom();

        yield return new WaitForSeconds(1);

        EndChain();
    }

    public void ChainOpportunity()
    {
        if (_player.Dead) ChangeState(Actions.Inactive);

        if (CanMelee())
        {
            _lookDir = (_player.transform.position - transform.position).MakeHorizontal();
            ChangeState(Actions.Melee);
        }

        if (Random.Range(0, 100) < _baseChainChance - _chainChanceReduction * _chainCounter)
        {
            var attackList = _chainableAttacks.OrderBy(x => Random.value).ToList();

            while (attackList.Any())
            {
                var currentAction = attackList.First();

                if (!_actionHistory.Contains(currentAction))
                {
                    if (TryChain(currentAction))
                    {
                        _actionHistory.Dequeue();
                        _actionHistory.Enqueue(currentAction);
                        _chainCounter++;
                        return;
                    }
                }

                attackList.RemoveAt(0);
            }
        }

        _chainCounter = 0;
    }

    public void ChainFailed()
    {
        if (_chainCounter <= 0) ChangeState(Actions.Approach);
    }

    public void EndChain()
    {
        _chainCounter = 0;
        ChangeState(Actions.Approach);
    }

    public void Melee()
    {
        Vector3 spawnPos;

        if (Physics.Raycast(transform.position, Vector3.down, out var hit, Mathf.Infinity, _groundLayer)) spawnPos = hit.point;
        else return;

        //var spike = Instantiate(_melee, spawnPos, transform.rotation);
        //spike.Play();

        StartCoroutine(MeleeHitCheck(1, _meleeDamage, spawnPos, _meleeRadius));
    }

    IEnumerator MeleeHitCheck(float duration, float damage, Vector3 center, float radius)
    {
        float timer = 0;
        List<Collider> ignore = new List<Collider>();
        bool skip = false;

        while (timer < duration)
        {
            var cols = Physics.OverlapSphere(center, radius, _spikeTargets);

            foreach (var item in cols)
            {
                foreach (var item2 in ignore)
                {
                    if (item == item2) skip = true;
                }

                if (skip)
                {
                    skip = false;
                    continue;
                }

                if (item.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);

                    if (item != null) ignore.Add(item);
                }
                else
                {
                    damageable = item.GetComponentInParent<IDamageable>();

                    if (damageable != null) damageable.TakeDamage(damage);

                    if (item != null) ignore.Add(item);
                }
            }

            timer += Time.deltaTime;

            yield return null;
        }
    }

    public void BudStrike()
    {
        Vector3 spawnPos;

        if (_player.Grounded) spawnPos = _player.transform.position;
        else
        {
            if (Physics.Raycast(_player.transform.position, Vector3.down, out var hit, Mathf.Infinity, _groundLayer)) spawnPos = hit.point;
            else return;
        }

        var bud = _budPool.Get();
        bud.Initialize(_budPool, _homingShardPool, BudDestroyed, spawnPos, _budStrikeDamage, _basicShardSpeed, _basicShardDamage);

        _spawnedBuds.Add(bud);

        //if (Physics.CheckCapsule(spawnPos, spawnPos + Vector3.up, 0.1f, _playerLayer))
        //{
        //    //_player.KnockBack(_player.transform.position - nextSpawnPos, _wallSpikeKnockback);
        //    _player.TakeDamage(_budStrikeDamage);
        //}
    }

    public void SpawnBud(Vector3 pos)
    {
        var bud = _budPool.Get();
        bud.Initialize(_budPool, _homingShardPool, BudDestroyed, pos, _budStrikeDamage, _basicShardSpeed, _basicShardDamage);

        _spawnedBuds.Add(bud);
    }

    public void Prebloom()
    {
        //_bloomingBuds = _spawnedBuds.Where(x => Vector3.Distance(_player.transform.position, x.transform.position) <= _budActivationRange).ToList();

        foreach (var item in _bloomingBuds)
        {
            _spawnedBuds.Remove(item);
            item.Prebloom();
        }
    }

    public void Bloom()
    {
        foreach (var item in _bloomingBuds)
        {
            item.Bloom(_player.target.position, _projectileAmount);
        }
    }

    public void BudDestroyed(ObsidianBud bud)
    {
        if (_spawnedBuds.Contains(bud)) _spawnedBuds.Remove(bud);
        else if (_bloomingBuds.Contains(bud)) _bloomingBuds.Remove(bud);
    }

    public void QuickShot()
    {
        var dir = _player.target.position - _quickShotSpawnPos.position;
        int count = _secondPhase ? _enhancedQuickShotCount : _quickShotCount;

        for (int i = 0; i < count; i++)
        {
            var shard = _basicShardPool.Get();
            shard.Initialize(_basicShardPool, _basicShardSpeed, _basicShardDamage, _player.target, SpawnBud);
            shard.transform.position = _quickShotSpawnPos.position;
            shard.transform.forward = dir.VectorVariation(i, _quickShotHorDirVariation, _quickShotVerDirVariation);
        }
    }

    public void SpawnHomingShards()
    {
        int count = _secondPhase ? _enhancedPowerShotCount : _powerShotCount;

        var baseAngle = 360 / count;
        var halfAngle = baseAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            var shard = _homingShardPool.Get();
            shard.transform.position = _powerShotSpawnPos.position;
            shard.transform.up = Vector3.up;
            shard.transform.rotation = Quaternion.Euler(new Vector3(0, baseAngle * i + Mathf.Lerp(0, halfAngle, Random.value)));
            shard.transform.position += shard.transform.forward * _powerShotSpawnOffset;
            shard.transform.forward = Vector3.up;
            shard.transform.SetParent(_powerShotSpawnPos);

            _homingShards.Add(shard);
        }
    }

    //public void ShootHomingShards()
    //{
    //    foreach (var item in _homingShards)
    //    {
    //        item.transform.parent = null;
    //        item.transform.forward = _player.target.position - item.transform.position;
    //        item.Initialize(_homingShardPool, _shardSpeed, _shardDamage, _player.target);
    //    }
    //
    //    _homingShards.Clear();
    //
    //    _trackPlayer = false;
    //}

    public void ShootHomingShards()
    {
        StartCoroutine(ShootingHomingShards());
    }

    IEnumerator ShootingHomingShards()
    {
        foreach (var item in _homingShards)
        {
            item.transform.parent = null;
            item.transform.forward = _player.target.position - item.transform.position;
            item.Initialize(_homingShardPool, _homingShardSpeed, _homingShardDamage, _player.target);

            yield return new WaitForSeconds(0.1f);
        }

        _homingShards.Clear();
    }

    public void KnifeThrustEvent()
    {
        _gKnife.Thrust();
    }

    public void KnifeSliceEvent()
    {
        _gKnife.Slice();
    }

    public void KnifeStopEvent()
    {
        _gKnife.StopHitting();
    }

    IEnumerator PlaceholderWallSpiking()
    {
        Vector3 target;

        if (_player.Grounded) target = _player.transform.position;
        else
        {
            if (Physics.Raycast(_player.transform.position, Vector3.down, out var hit, Mathf.Infinity, _groundLayer)) target = hit.point;
            else yield break;
        }

        Vector3 dir = (target - transform.position).MakeHorizontal().normalized;
        Vector3 startPos = transform.position + dir * _wallSpikeSpawnOffset;

        var vfx = Instantiate(_movingSpikes, startPos, Quaternion.identity);
        vfx.transform.forward = dir;

        while (Vector3.Distance(vfx.transform.position, target) > 0.5f)
        {
            vfx.transform.position += dir * _wallSpikeTravelSpeed * Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(_wallSpawnDelay);

        var wall = Instantiate(_wallPrefab, target, Quaternion.identity);

        if (Physics.CheckCapsule(target, target + Vector3.up * 5, wall.Radius, _playerLayer))
        {
            _player.KnockBack(_player.transform.position - target, _wallSpikeKnockback);
            _player.TakeDamage(_wallSpikeDamage);
        }

        yield return new WaitForSeconds(5);

        Destroy(vfx.gameObject);
    }

    IEnumerator SpikesHitCheck(float duration, float damage, Vector3 center, float boxX, float boxY, float boxZ, Quaternion orientation, bool stoppable = true)
    {
        float timer = 0;
        List<Collider> ignore = new List<Collider>();
        bool skip = false;

        var cols = Physics.OverlapBox(center, new Vector3(boxX, boxY, boxZ), orientation, _obstacleLayer);

        if (stoppable && cols.Any()) _stopGroundSpikes = true;

        while (timer < duration)
        {
            cols = Physics.OverlapBox(center, new Vector3(boxX, boxY, boxZ), orientation, _spikeTargets);

            foreach (var item in cols)
            {
                foreach (var item2 in ignore)
                {
                    if (item == item2) skip = true;
                }

                if (skip)
                {
                    skip = false;
                    continue;
                }

                if (item.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);

                    if (item != null) ignore.Add(item);
                }
                else
                {
                    damageable = item.GetComponentInParent<IDamageable>();

                    if (damageable != null) damageable.TakeDamage(damage);

                    if (item != null) ignore.Add(item);
                }
            }

            timer += Time.deltaTime;

            yield return null;
        }
    }

    public void StartGroundSpikes()
    {
        _trackPlayer = false;

        Vector3 spawnPos, targetPos, dir;

        if (Physics.Raycast(_player.transform.position + Vector3.up, Vector3.down, out var hit, Mathf.Infinity, _groundLayer)) targetPos = hit.point;
        else return;

        dir = (targetPos - transform.position).MakeHorizontal().normalized;
        Quaternion rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, Mathf.Infinity, _groundLayer)) spawnPos = hit.point + dir * _spikesStartOffset;
        else return;

        StartCoroutine(ChainSpikes(1, spawnPos, rotation, 0, _spikesDamage * 3, true, true, false));
    }

    IEnumerator ChainSpikes(int rowNumber, Vector3 spawnPos, Quaternion orientation, float spawnDelay, float damage, bool chainLeft = false, bool chainRight = false, bool stoppable = true)
    {
        yield return new WaitForSeconds(spawnDelay);

        var spikes = _spikesPool.Get();
        //spikes.Initialize(spawnPos, orientation);

        var obstacles = Physics.OverlapBox(spawnPos, new Vector3(_spikesOffset, _spikesSizeY, _spikesOffset), orientation, _obstacleLayer);

        if (!stoppable || !obstacles.Any() && rowNumber < _spikesMaxRows)
        {
            StartCoroutine(ChainSpikes(rowNumber + 1, spawnPos + spikes.transform.forward * _spikesOffset, orientation, _spikesDelay, _spikesDamage));

            if (chainLeft) StartCoroutine(ChainSpikes(rowNumber + 1, spawnPos + spikes.transform.forward * _spikesOffset - spikes.transform.right * _spikesOffset, orientation, _spikesDelay, _spikesDamage, true));

            if (chainRight) StartCoroutine(ChainSpikes(rowNumber + 1, spawnPos + spikes.transform.forward * _spikesOffset + spikes.transform.right * _spikesOffset, orientation, _spikesDelay, _spikesDamage, false, true));
        }

        float timer = 0;
        List<Collider> ignore = new List<Collider>();
        bool skip = false;

        while (timer < _spikesDuration)
        {
            var cols = Physics.OverlapBox(spawnPos, new Vector3(_spikesOffset, _spikesSizeY, _spikesOffset), orientation, _spikeTargets);

            foreach (var item in cols)
            {
                foreach (var item2 in ignore)
                {
                    if (item == item2) skip = true;
                }

                if (skip)
                {
                    skip = false;
                    continue;
                }

                if (item.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);

                    if (item != null) ignore.Add(item);
                }
                else
                {
                    damageable = item.GetComponentInParent<IDamageable>();

                    if (damageable != null) damageable.TakeDamage(damage);

                    if (item != null) ignore.Add(item);
                }
            }

            timer += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(spikeDespawnTime);

        _spikesPool.RefillStock(spikes);
    }

    public void NewGroundSpikes(int footIndex)
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out var hit, Mathf.Infinity, _groundLayer)) return;

        float dirOffset = _totalAngle / (_spikeCount - 1);

        Vector3 baseDir = transform.eulerAngles - new Vector3(0, _totalAngle * 0.5f);

        for (int i = 0; i < _spikeCount; i++)
        {
            var spike = _spikesPool.Get();
            spike.Initialize(_spikesPool, new Vector3(_footPos[footIndex].transform.position.x, hit.point.y, _footPos[footIndex].transform.position.z), Quaternion.Euler(baseDir + new Vector3(0, dirOffset * i)), _spikesSpeed, _spikesDamage);
        }
    }

    public void StartLimbVFX()
    {
        _anim.SetTrigger("StartLimbVFX");
    }

    public void LimbImpact()
    {
        _limbExplosion.transform.parent = null;
        _limb.SetActive(false);
        _limbExplosion.ToggleGameObject(this, true, 2);

        var cols = Physics.OverlapBox(transform.position + transform.forward * _limbCheckBoxSize.z, _limbCheckBoxSize, transform.rotation, _spikeTargets);

        foreach (var item in cols)
        {
            if (item.TryGetComponent(out IDamageable damageable))
            {
                if (item.TryGetComponent(out NebulaShield nebula))
                {
                    nebula.Overcharge();

                    continue;
                }

                damageable.TakeDamage(_limbDamage);
            }
            else
            {
                damageable = item.GetComponentInParent<IDamageable>();

                if (damageable != null) damageable.TakeDamage(_limbDamage);
            }
        }
    }

    public override void TakeDamage(float amount, bool bypassCooldown = false)
    {
        base.TakeDamage(amount);
        Debug.Log(_hp);
        UIManager.instance.UpdateBar(UIManager.Bar.BossHp, _hp);

        if (!_secondPhase && _hp <= _maxHp * 0.5f)
        {
            _secondPhase = true;

            _currentLimbWindUp = _enhancedLimbWindUpDuration;
            _bodyMaterial.SetFloat("_Emission_Intensity_Min", 15f);
            _bodyMaterial.SetVector("_AMP", new Vector4(-1.13f, 0.05f, 1));
            _bodyMaterial.SetVector("_AMP_2", new Vector4(0.25f, 0.05f, 1));
            _bodyMaterial.SetColor("_Emission", Color.red);
            _anim.SetFloat("speedMultiplier", 1.6f);
        }
    }

    public override void Die()
    {
        StopAllCoroutines();
        _anim.SetBool("Dead", true);

        _player.Inputs.inputUpdate = _player.Inputs.Nothing;

        UIManager.instance.ToggleBossBar(false);
        UIManager.instance.HideUI(true);

        _deathCam.SetActive(true);

        //_player.Inputs.inputUpdate = _player.Inputs.Nothing;
        //_outroTimeline.Play();

        //prenderCaidaPiedras(true);
        //CineManager.instance.PlayAnimation(CineManager.cineEnum.obsidianDead);
        //Destroy(gameObject);
    }

    public void OnDeathAnimEnd()
    {
        UIManager.instance.StartPlaceholderDemoEnd();
    }
    public void MoveObelisc()
    {
        _obelisc.SetVector3("Position1",new Vector3(transform.position.x, 91.8f, transform.position.z));
    }
}