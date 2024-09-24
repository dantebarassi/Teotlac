using IA2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;

public class Itztlacoliuhqui : Boss
{
    public enum Actions
    {
        Inactive,
        Search,
        Spikes,
        Swing,
        BreakWall,
        Shield,
        Hide,
        WallSpike,
        Gatling,
        Charge,
        ArenaSpikes,
        Leap
    }

    EventFSM<Actions> _fsm;

    BehaviorNode _treeStart;

    Pathfinding _pf;
    List<Vector3> _path = new List<Vector3>();

    [SerializeField] bool _playOnStart;
    [SerializeField] Animator _anim;
    [SerializeField] float _aggroRange;
    [SerializeField] Transform _eyePos;
    [SerializeField] GameObject _edgeBlock, _preSpikeParticle;
    [SerializeField] Actions[] _creationAttacks, _attacks;

    [Header("Walls")]
    [SerializeField] ObsidianWall _wallPrefab;
    [SerializeField] List<ObsidianWall> _spawnedWalls;
    [SerializeField] LayerMask _wallLayer;
    [SerializeField] float _wallCloseRange, _wallBreakRange, _playerCloseRange;

    [Header("Shards")]
    [SerializeField] Projectile _shardPrefab;
    [SerializeField] float _shardSpeed, _shardDamage;

    [Header("Search")]
    [SerializeField] float _searchSpeed;

    [Header("Spikes")]
    [SerializeField] Hazard _spikes;
    [SerializeField] ParticleSystem[] _preJumpParticles;
    [SerializeField] float _spikesPreparation, _spikesDuration, _spikesDamage, _spikesRecovery;

    [Header("Break Wall")]
    [SerializeField] int _shardAmount;
    [SerializeField] float _breakWallPreparation, _breakWallRecovery, _breakBaseSpawnOffsetY, _breakSpawnVariationY, _breakAimVariationX, _breakAimVariationY;

    [Header("Shield")]
    [SerializeField] float _shieldPreparation;
    [SerializeField] float _shieldRecovery, _forwardOffset;

    [Header("Hide")]
    [SerializeField] float _hideSpeed;
    [SerializeField] float _hideDuration;

    [Header("Wall Spike")]
    [SerializeField] GameObject _miniWall;
    [SerializeField] float _firstWallOffset, _miniWallOffset, _miniWallInterval, _wallSpikeKnockback,
                           _wallSpikeDamage, _wallSpikePreparation, _wallSpikeRecovery, _miniWallDestroyDelay;

    [Header("Gatling")]
    [SerializeField] Transform _handPos;
    [SerializeField] float _gatlingDuration, _gatlingShardInterval, _gatlingPreparation, _gatlingRecovery, _gatlingSpawnVariationX, _gatlingSpawnVariationY, _gatlingAimVariationX, _gatlingAimVariationY;

    [Header("Dash (Placeholder)")]
    [SerializeField] float _dashStrength;
    [SerializeField] float _dashPreparation, _dashDuration, _dashRecovery;

    [Header("Charge")]
    [SerializeField] Vector3 _chargeBoxSize;
    [SerializeField] float _chargeSpeed, _chargeHitRange, _chargeDamage, _chargeKnockback, _chargePreparation, _chargeRecovery;

    [Header("Arena Spikes")]
    [SerializeField] GameObject _arenaSpikePrefab;
    [SerializeField] ParticleSystem[] _preArenaSpikesParticles;
    [SerializeField] int _attacksToProcArenaSpikes;
    [SerializeField] float _arenaSpikeInterval, _arenaSpikeDamage, _arenaSpikesPreparation, _arenaSpikeLinger, _arenaSpikesRecovery;

    [Header("Leap")]
    [SerializeField] float _leapHeight;
    [SerializeField] float _leapShardAmount, _leapLandingDamageRadius, _leapKnockback, _leapDamage, _leapPreparation, _leapDuration, _leapRecovery;

    [SerializeField] PlayerController _player;
    [SerializeField] LayerMask _playerLayer, _magicLayer;
    
    ObsidianWall _wallBlockingLOS;

    ObsidianPathfindManager _pfManager { get => ObsidianPathfindManager.instance; }

    bool _takingAction = false, _lookAtPlayer = false, _move = false, _start = false;

    float _timer = 0, _currentSpeed = 0;
    int _attackCounter;

    //[SerializeField] Animator anim;

    AudioSource _myAS;
    [SerializeField] AudioClip stomp, dash, dashBox, dashFuerte, lanzaDardos, walk, RunTowards, PinchosPiso, TemblorArenaSpiking;
    [SerializeField] AudioClip[] _gatlingSounds;

    [SerializeField] GameObject tornadoPiedras, caidaPiedras;

    [SerializeField] PlayableDirector _outroTimeline;

    public bool LookAtPlayer
    {
        get
        {
            return _lookAtPlayer;
        }

        set
        {
            //_rb.constraints = value ? RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ : RigidbodyConstraints.FreezeRotation;
            _lookAtPlayer = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        UIManager.instance.UpdateBar(UIManager.Bar.BossHp, _hp, _maxHp);

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
        var search = new State<Actions>("Search");
        var spikes = new State<Actions>("Spikes");
        var swing = new State<Actions>("Swing");
        var breakWall = new State<Actions>("BreakWall");
        var shield = new State<Actions>("Shield");
        var hide = new State<Actions>("Hide");
        var wallSpike = new State<Actions>("WallSpike");
        var gatling = new State<Actions>("Gatling");
        var charge = new State<Actions>("Charge");
        var arenaSpikes = new State<Actions>("ArenaSpikes");
        var leap = new State<Actions>("Leap");

        StateConfigurer.Create(inactive)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(search)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(spikes)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(swing)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(breakWall)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(shield)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(hide)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(wallSpike)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(gatling)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(charge)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(arenaSpikes)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        StateConfigurer.Create(leap)
            .SetTransition(Actions.Search, search)
            .SetTransition(Actions.Spikes, spikes)
            .SetTransition(Actions.Swing, swing)
            .SetTransition(Actions.BreakWall, breakWall)
            .SetTransition(Actions.Shield, shield)
            .SetTransition(Actions.Hide, hide)
            .SetTransition(Actions.WallSpike, wallSpike)
            .SetTransition(Actions.Gatling, gatling)
            .SetTransition(Actions.Charge, charge)
            .SetTransition(Actions.ArenaSpikes, arenaSpikes)
            .SetTransition(Actions.Leap, leap)
            .Done();

        #endregion

        #region FSM State Setup

        inactive.OnUpdate += () =>
        {
            if (Vector3.Distance(transform.position, _player.transform.position) <= _aggroRange)
            {
                UIManager.instance.UpdateBar(UIManager.Bar.BossHp, _hp, _maxHp);
                UIManager.instance.ChangeBossName("Itztlacoliuhqui, God of Obsidian");
                _player.FightStarts(this);
                _edgeBlock.SetActive(true);
                UIManager.instance.ToggleBossBar(true);
                _treeStart.Execute();
            }
        };

        search.OnEnter += x =>
        {
            Debug.Log("Start search");
            _anim.SetBool("IsWalking", true);
            _currentSpeed = _searchSpeed;
            _path = _pf.ThetaStar(_pfManager.FindNodeClosestTo(transform.position), _pfManager.FindNodeClosestTo(_player.transform.position), _wallLayer);
            _move = true;
        };

        search.OnUpdate += () =>
        {
            if (transform.position.InLineOfSightOf(_player.transform.position, _wallLayer) || _path.Count == 0 || _path == null)
            {
                _treeStart.Execute();
            }
        };

        search.OnFixedUpdate += () =>
        {
            TravelPath();
        };

        search.OnExit += x =>
        {
            _anim.SetBool("IsWalking", false);
            _currentSpeed = 0;
            _move = false;
        };

        spikes.OnEnter += x =>
        {
            _attackCounter++;
            Debug.Log("Start spikes");
            _takingAction = true;
            _anim.SetBool("IsStomp", true);
            StartCoroutine(Spiking());
        };

        spikes.OnUpdate += () =>
        {
            if (!_takingAction) _treeStart.Execute();
        };

        spikes.OnExit += x =>
        {
            _anim.SetBool("IsStomp", false);
        };

        breakWall.OnEnter += x =>
        {
            _attackCounter++;
            Debug.Log("Start break wall");
            _anim.SetBool("IsShooting", true);
            _takingAction = true;
            StartCoroutine(BreakingWall());
        };

        breakWall.OnUpdate += () =>
        {
            //_anim.SetBool("IsShooting", false);
            if (!_takingAction) _treeStart.Execute();
        };
        breakWall.OnExit += x =>
        {
            _anim.SetBool("IsShooting", false);
        };

        shield.OnEnter += x =>
        {
            Debug.Log("Start shield");
            _anim.SetBool("IsBoxAttack", true);
            _takingAction = true;
            StartCoroutine(Shielding());
        };

        shield.OnUpdate += () =>
        {
            if (!_takingAction) _treeStart.Execute();
        };

        shield.OnExit += x =>
        {
            _anim.SetBool("IsBoxAttack", false);
        };

        hide.OnEnter += x =>
        {
            Debug.Log("Start hide");
            _currentSpeed = _hideSpeed;
            _timer = 0;
            var closestWall = _spawnedWalls.Where(x => !x.Broken).OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
            Vector3 hidingSpot = closestWall.transform.position + (_player.transform.position - closestWall.transform.position).normalized * -closestWall.Radius;
            _path = _pf.ThetaStar(_pfManager.FindNodeClosestTo(transform.position), _pfManager.FindNodeClosestTo(hidingSpot), _wallLayer);
            _move = true;
        };

        hide.OnUpdate += () =>
        {
            if (_timer < _hideDuration)
            {
                _timer += Time.deltaTime;
            }
            else
            {
                _treeStart.Execute();
            }
        };

        hide.OnFixedUpdate += () =>
        {
            if (_path.Count == 0 || _path == null) return;
            TravelPath();
        };

        hide.OnExit += x =>
        {
            _currentSpeed = 0;
            _move = false;
        };

        wallSpike.OnEnter += x =>
        {
            _attackCounter++;
            _anim.SetBool("IsBoxAttack", true);
            Debug.Log("Start wall spike");
            _takingAction = true;
            StartCoroutine(WallSpiking());
        };

        wallSpike.OnUpdate += () =>
        {
            if (!_takingAction) _treeStart.Execute();
        };

        wallSpike.OnExit += x =>
        {
            _anim.SetBool("IsBoxAttack", false);
        };

        gatling.OnEnter += x =>
        {
            _attackCounter++;
            Debug.Log("Start gatling");
            _anim.SetBool("IsBoxAttack", true);
            _takingAction = true;
            StartCoroutine(UsingGatling());
        };

        gatling.OnUpdate += () =>
        {
            if (!_takingAction) _treeStart.Execute();
        };

        gatling.OnExit += x =>
        {
            _anim.SetBool("IsBoxAttack", false);
        };

        charge.OnEnter += x =>
        {
            _attackCounter++;
            Debug.Log("Start charge");
            //_anim.SetBool("IsDashing", true);
            _takingAction = true;
            StartCoroutine(Charging());
        };

        charge.OnUpdate += () =>
        {
            if (!_takingAction) _treeStart.Execute();
        };

        charge.OnExit += x =>
        {
            //_anim.SetBool("IsDashing", false);
        };

        arenaSpikes.OnEnter += x =>
        {
            _attackCounter = 0;
            _takingAction = true;
            StartCoroutine(ArenaSpiking());
        };

        arenaSpikes.OnUpdate += () =>
        {
            if (!_takingAction) _treeStart.Execute();
        };

        leap.OnEnter += x =>
        {
            _attackCounter++;
            //_anim.SetBool("IsStomp", true);
            _takingAction = true;
            StartCoroutine(Leaping());
        };

        leap.OnUpdate += () =>
        {
            if (!_takingAction) _treeStart.Execute();
        };

        leap.OnExit += x =>
        {
            _anim.SetBool("IsStomp", false);
        };

        #endregion

        _fsm = new EventFSM<Actions>(inactive);

        #region Decision Tree Setup

        var searchNode = new ActionNode(Search);
        var spikesNode = new ActionNode(Spikes);
        //var swingNode = new ActionNode(Swing);
        var breakWallNode = new ActionNode(BreakWall);
        var shieldNode = new ActionNode(Shield);
        var hideNode = new ActionNode(Hide);
        //var wallSpikeNode = new ActionNode(WallSpike);
        //var gatlingNode = new ActionNode(Gatling);
        //var chargeNode = new ActionNode(Charge);
        var creationAttacks = new ActionNode(WallCreationAttacks);
        var attacks = new ActionNode(Attacks);
        var arenaSpikesNode = new ActionNode(ArenaSpikes);

        //var wallCloseToPlayer = new QuestionNode(gatlingNode, wallSpikeNode, IsWallCloseToPlayer);
        var spawnWall = new QuestionNode(creationAttacks, attacks, ShouldSpawnWalls);
        var anyWallClose = new QuestionNode(spawnWall, shieldNode, IsAnyWallClose);
        var unbrokenWallClose = new QuestionNode(hideNode, anyWallClose, IsUnbrokenWallClose);
        var defend = new QuestionNode(unbrokenWallClose, spawnWall, ShouldDefend);
        var playerSunning = new QuestionNode(defend, spawnWall, IsPlayerUsingSun);
        var playerInWallLOS = new QuestionNode(breakWallNode, searchNode, BreakableWallInPlayerLOS);
        var playerClose = new QuestionNode(spikesNode, playerSunning, IsPlayerClose);
        var breakWallInLOS = new QuestionNode(playerInWallLOS, searchNode, CanBreakWallBlockingLOS);
        var playerInLOS = new QuestionNode(playerClose, breakWallInLOS, IsPlayerInLOS);
        _treeStart = new QuestionNode(arenaSpikesNode, playerInLOS, ShouldUseUltimate);

        #endregion

        _start = true;
    }

    private void Update()
    {
        if (!_start) return;

        _fsm.Update();
    }

    private void FixedUpdate()
    {
        if (!_start) return;

        _fsm.FixedUpdate();

        if (_move)
        {
            _rb.MovePosition(transform.position + transform.forward * _currentSpeed * Time.fixedDeltaTime);
        }
        if (LookAtPlayer)
        {
            _rb.MoveRotation(Quaternion.LookRotation((_player.transform.position - transform.position).MakeHorizontal()));
        }
    }

    #region Decision Tree Methods

    void WallCreationAttacks() => _fsm.SendInput(_creationAttacks[Random.Range(0, _creationAttacks.Length)]);
    void Attacks() => _fsm.SendInput(_attacks[Random.Range(0, _creationAttacks.Length)]);

    void Search() => _fsm.SendInput(Actions.Search);
    void Spikes() => _fsm.SendInput(Actions.Spikes);
    void Swing() => _fsm.SendInput(Actions.Swing);
    void BreakWall() => _fsm.SendInput(Actions.BreakWall);
    void Shield() => _fsm.SendInput(Actions.Shield);
    void Hide() => _fsm.SendInput(Actions.Hide);
    void WallSpike() => _fsm.SendInput(Actions.WallSpike);
    void Gatling() => _fsm.SendInput(Actions.Gatling);
    void Charge() => _fsm.SendInput(Actions.Charge);
    void ArenaSpikes() => _fsm.SendInput(Actions.ArenaSpikes);
    void Leap() => _fsm.SendInput(Actions.Leap);

    bool ShouldUseUltimate()
    {
        return _attackCounter >= _attacksToProcArenaSpikes;
    }
    bool IsAnyWallClose()
    {
        foreach (var item in _spawnedWalls)
        {
            if (Vector3.Distance(transform.position, item.transform.position) <= _wallCloseRange)
            {
                return true;
            }
        }

        return false;
    }
    bool IsUnbrokenWallClose()
    {
        foreach (var item in _spawnedWalls.Where(x => !x.Broken))
        {
            if (Vector3.Distance(transform.position, item.transform.position) <= _wallCloseRange)
            {
                return true;
            }
        }

        return false;
    }
    bool IsWallCloseToPlayer()
    {
        foreach (var item in _spawnedWalls)
        {
            if (Vector3.Distance(_player.transform.position, item.transform.position) <= _wallCloseRange)
            {
                return true;
            }
        }

        return false;
    }
    bool ShouldDefend() => Random.Range(0, 2) == 0;
    bool IsPlayerUsingSun() => _player.Aiming;
    bool BreakableWallInPlayerLOS()
    {
        _player.transform.position.InLineOfSightOf(_wallBlockingLOS.transform.position, _wallLayer, out var hit);

        if (hit.collider.GetComponentInParent<ObsidianWall>() == _wallBlockingLOS)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    bool CanBreakWallBlockingLOS() => Vector3.Distance(transform.position, _wallBlockingLOS.transform.position) <= _wallBreakRange && !_wallBlockingLOS.Broken;
    bool IsPlayerClose() => Vector3.Distance(transform.position, _player.transform.position) <= _playerCloseRange;
    bool IsPlayerInLOS()
    {
        if (!_eyePos.position.InLineOfSightOf(_player.transform.position + Vector3.up, _wallLayer, out var hit))
        {
            _wallBlockingLOS = hit.collider.GetComponentInParent<ObsidianWall>();
            return false;
        }
        else
        {
            _wallBlockingLOS = null;
            return true;
        }
    }
    bool ShouldSpawnWalls()
    {
        return Random.Range(0, 100) > _spawnedWalls.Count * _spawnedWalls.Count;
    }

    #endregion

    void FixRotation(bool fix)
    {
        _rb.constraints = fix ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void TravelPath()
    {
        Vector3 posTarget = _path[0];
        posTarget.y = transform.position.y;
        Vector3 dir = posTarget - transform.position;
        if (dir.magnitude < 0.1f)
        {
            _rb.MovePosition(posTarget);
            _path.RemoveAt(0);

            if (_path.Count == 0)
            {
                _currentSpeed = 0;
                _move = false;
            }
        }

        _rb.MoveRotation(Quaternion.LookRotation(dir.MakeHorizontal()));
        
    }

    public void WallDestroyed(ObsidianWall wall)
    {
        if (_spawnedWalls.Contains(wall)) _spawnedWalls.Remove(wall);
    }

    IEnumerator Spiking()
    {
        //LookAtPlayer = false;
        //ChangeAudio(dash);
        //preparacion
        for (int i = 0; i < _preJumpParticles.Length; i++)
        {
            _preJumpParticles[i].Play();
        }
        prenderCaidaPiedras(true);
        ChangeAudio(dash);
        yield return new WaitForSeconds(_spikesPreparation);
        ChangeAudio(stomp);
        //salen spikes del suelo
        //ChangeAudio(stomp);
        prenderCaidaPiedras(false);
        var spikes = Instantiate(_spikes, transform.position, transform.rotation);
        spikes.duration = _spikesDuration;
        spikes.damage = _spikesDamage;
        for (int i = 0; i < _preJumpParticles.Length; i++)
        {
            _preJumpParticles[i].Stop();
        }
        //recuperacion
        yield return new WaitForSeconds(_spikesRecovery);

        //LookAtPlayer = true;
        _takingAction = false;
    }

    IEnumerator BreakingWall()
    {
        _rb.MoveRotation(Quaternion.LookRotation((_wallBlockingLOS.transform.position - transform.position).MakeHorizontal()));
        prenderTornado(true);
        yield return new WaitForSeconds(_breakWallPreparation);
        ChangeAudio(dashBox);
        if (!_wallBlockingLOS.Broken && !IsPlayerInLOS())
        {
            _wallBlockingLOS.Break();

            Vector3 basePos = new Vector3(_wallBlockingLOS.transform.position.x, transform.position.y + _breakBaseSpawnOffsetY, _wallBlockingLOS.transform.position.z);
            float xPosVariation = _wallBlockingLOS.Radius;
            Vector3 baseDir = (_player.transform.position + Vector3.up - basePos).normalized;

            for (int i = 0; i < _shardAmount; i++)
            {
                var shard = Instantiate(_shardPrefab, basePos.VectorVariation(1, xPosVariation, _breakSpawnVariationY), Quaternion.identity);
                shard.transform.forward = baseDir.VectorVariation(i * 0.5f, _breakAimVariationX, _breakAimVariationY);
                shard.speed = _shardSpeed;
                shard.damage = _shardDamage;
            }
            prenderTornado(false);
            yield return new WaitForSeconds(_breakWallRecovery);
        }
       
        //LookAtPlayer = true;
        _takingAction = false;
    }

    IEnumerator Shielding()
    {
        LookAtPlayer = true;
        prenderTornado(true);
        yield return new WaitForSeconds(_shieldPreparation);
        ChangeAudio(dashBox);
        var wall = Instantiate(_wallPrefab, transform.position + transform.forward * _forwardOffset - Vector3.up * _distFromPivotToFloor, Quaternion.identity);
        _spawnedWalls.Add(wall);
        wall.boss = this;
        prenderTornado(false);
        _anim.SetBool("IsBoxAttack", false);
        yield return new WaitForSeconds(_shieldRecovery);

        LookAtPlayer = false;
        _takingAction = false;
    }

    IEnumerator WallSpiking()
    {
        LookAtPlayer = true;
        prenderTornado(true);
        yield return new WaitForSeconds(_wallSpikePreparation);
        ChangeAudio(PinchosPiso);
        LookAtPlayer = false;

        Vector3 target = _player.transform.position;
        Vector3 dir = (target - transform.position).MakeHorizontal().normalized;

        List<GameObject> miniWallList = new List<GameObject>();

        Vector3 lastPos, nextSpawnPos = transform.position - transform.up * _distFromPivotToFloor + dir * _firstWallOffset;
        Vector3 movement = dir * _miniWallOffset;
        target = new Vector3(target.x, nextSpawnPos.y, target.z);

        var particles = Instantiate(_preSpikeParticle, nextSpawnPos, transform.rotation);
        float timer;

        do
        {
            var miniWall = Instantiate(_miniWall, nextSpawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            miniWall.transform.localScale *= Random.Range(0.9f, 1.2f);
            miniWallList.Add(miniWall);
            lastPos = nextSpawnPos;
            nextSpawnPos += movement;

            timer = 0;

            while (timer < _miniWallInterval)
            {
                timer += Time.deltaTime;

                particles.transform.position = Vector3.Lerp(lastPos, nextSpawnPos, timer / _miniWallInterval);

                yield return null;
            }
        }
        while (Vector3.Distance(nextSpawnPos, target) > _wallPrefab.Radius);

        timer = 0;
        var wait = _miniWallInterval * 2;

        while (timer < wait)
        {
            timer += Time.deltaTime;

            particles.transform.position = Vector3.Lerp(nextSpawnPos, target, timer / wait);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        nextSpawnPos = target;
        var wall = Instantiate(_wallPrefab, nextSpawnPos, Quaternion.identity);
        _spawnedWalls.Add(wall);
        wall.boss = this;

        Destroy(particles);

        if (Physics.CheckCapsule(nextSpawnPos, nextSpawnPos + Vector3.up * 5, wall.Radius, _playerLayer))
        {
            _player.KnockBack(_player.transform.position - nextSpawnPos, _wallSpikeKnockback);
            _player.TakeDamage(_wallSpikeDamage);
        }
        prenderTornado(false);
        yield return new WaitForSeconds(_wallSpikeRecovery);

        _takingAction = false;

        yield return new WaitForSeconds(_miniWallDestroyDelay);

        foreach (var item in miniWallList)
        {
            Destroy(item);
        }
    }

    IEnumerator UsingGatling()
    {
        LookAtPlayer = true;
        prenderTornado(true);
        yield return new WaitForSeconds(_gatlingPreparation);
        
        float timer = 0, cooldown = 0;

        while (timer < _gatlingDuration)
        {
            if (cooldown >= _gatlingShardInterval)
            {
                ChangeAudio(_gatlingSounds[Random.Range(0, _gatlingSounds.Length)]);

                var pos = _handPos.transform.position;
                var dir = (_player.transform.position + Vector3.up - pos).normalized;

                var shard = Instantiate(_shardPrefab, pos.VectorVariation(1, _gatlingSpawnVariationX, _gatlingSpawnVariationY), Quaternion.identity);
                shard.transform.forward = dir.VectorVariation(1, _gatlingAimVariationX, _gatlingAimVariationY);
                shard.speed = _shardSpeed;
                shard.damage = _shardDamage;
                cooldown = 0;
            }
            else
            {
                cooldown += Time.deltaTime;
            }

            timer += Time.deltaTime;

            yield return null;
        }
        prenderTornado(false);
        yield return new WaitForSeconds(_gatlingRecovery);

        LookAtPlayer = false;
        _takingAction = false;
    }

    IEnumerator Dashing()
    {
        LookAtPlayer = true;
        //preparacion
        yield return new WaitForSeconds(_dashPreparation);
        //comienza dash
        //ChangeAudio(dash);
        _rb.AddForce(transform.forward * _dashStrength);

        LookAtPlayer = false;
        FixRotation(true);

        float timer = 0;

        while (timer < _dashDuration)
        {
            timer += Time.deltaTime;

            yield return null;
        }
        //termina dash
        _rb.velocity = Vector3.zero;
        //espero
        yield return new WaitForSeconds(_dashRecovery);

        //LookAtPlayer = true;
        FixRotation(false);
        _takingAction = false;
    }

    IEnumerator Charging()
    {
        LookAtPlayer = true;
        //preparacion
        prenderCaidaPiedras(true);
        yield return new WaitForSeconds(_chargePreparation);
        ChangeAudio(RunTowards);
        LookAtPlayer = false;
        FixRotation(true);
        _anim.SetBool("IsDashing", true);

        _currentSpeed = _chargeSpeed;
        _move = true;

        while (_move)
        {
            if (Physics.BoxCast(transform.position, _chargeBoxSize, transform.forward, out var hit, transform.rotation, _chargeHitRange))
            {
                if (hit.collider.gameObject.layer == Mathf.Log(_wallLayer.value, 2))
                {
                    Debug.Log("hit wall");
                    var wallHit = hit.collider.GetComponentInParent<ObsidianWall>();
                    var broken = wallHit.Broken;

                    wallHit.Die();

                    if (!broken)
                    {
                        _move = false;
                    }
                }
                else if (hit.collider.gameObject.layer == Mathf.Log(_playerLayer.value, 2))
                {
                    Debug.Log("hit player");
                    _player.KnockBack((_player.transform.position - transform.position + transform.up).normalized, _chargeKnockback);
                    _player.TakeDamage(_chargeDamage);
                    _move = false;
                }
                else if (hit.collider.gameObject.layer != Mathf.Log(_magicLayer.value, 2))
                {
                    if (hit.collider.TryGetComponent(out IDamageable damageable)) damageable.TakeDamage(_chargeDamage);
                    _move = false;
                }
            }

            yield return null;
        }
        _anim.SetBool("IsDashing", false);
        prenderCaidaPiedras(false);
        //espero
        yield return new WaitForSeconds(_chargeRecovery);

        //LookAtPlayer = true;
        FixRotation(false);
        _takingAction = false;
    }

    IEnumerator ArenaSpiking()
    {
        _anim.SetBool("IsRoaring", true);
        ChangeAudio(TemblorArenaSpiking);

        var wait = _arenaSpikesPreparation * 0.5f;

        yield return new WaitForSeconds(wait);

        for (int i = 0; i < _preJumpParticles.Length; i++)
        {
            _preArenaSpikesParticles[i].Play();
        }

        yield return new WaitForSeconds(wait);

        var nodeList = _pfManager.allNodes.Where(x => !x.isBlocked).OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).SkipWhile(x => Vector3.Distance(x.transform.position, transform.position) < 3).ToList();

        int distMultiplier = 0;
        bool hit = false;

        List<GameObject> spawnedSpikes = new();
        
        while (nodeList.Any())
        {
            distMultiplier++;

            var spawnAt = nodeList.TakeWhile(x => Vector3.Distance(x.transform.position, transform.position) < distMultiplier * _pfManager.neighborDistance + _pfManager.neighborDistance * 0.4f);

            if (spawnAt.Any())
            {
                foreach (var item in spawnAt)
                {
                    Vector3 pos = item.transform.position + new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

                    var spike = Instantiate(_arenaSpikePrefab, pos, Quaternion.Euler(new Vector3(0, Random.Range(0f, 360f))));
                    //spike.transform.localScale *= 5;
                    spawnedSpikes.Add(spike);

                    if (!hit && Physics.CheckCapsule(pos, pos + Vector3.up * 5, _pfManager.neighborDistance * 0.45f, _playerLayer))
                    {
                        _player.KnockBack(_player.transform.position - pos, _wallSpikeKnockback);
                        _player.TakeDamage(_arenaSpikeDamage);
                        hit = true;
                    }
                }

                nodeList.RemoveRange(0, spawnAt.Count());

                yield return new WaitForSeconds(_arenaSpikeInterval);
            }
        }

        for (int i = 0; i < _preJumpParticles.Length; i++)
        {
            _preArenaSpikesParticles[i].Stop();
        }

        yield return new WaitForSeconds(_arenaSpikeLinger);

        foreach (var item in spawnedSpikes)
        {
            Destroy(item);
        }

        _anim.SetBool("IsRoaring", false);

        yield return new WaitForSeconds(_arenaSpikesRecovery);

        _takingAction = false;
    }

    IEnumerator Leaping()
    {
        LookAtPlayer = true;
        //preparacion
        prenderCaidaPiedras(true);
        ChangeAudio(dash);
        yield return new WaitForSeconds(_leapPreparation);

        var wall = Instantiate(_wallPrefab, transform.position, Quaternion.identity);
        _spawnedWalls.Add(wall);
        wall.boss = this;

        _anim.SetBool("IsStomp", true);
        LookAtPlayer = false;
        FixRotation(true);
        _rb.isKinematic = true;

        ObsidianWall wallToDestroy = null;
        Vector3 startPos = transform.position, slamPos, horPos;
        float timer = 0, yPos, highestPoint = startPos.y + _leapHeight;
        
        if (_player.Grounded && Mathf.Abs(_player.transform.position.y - startPos.y) > 1f)
        {
            wallToDestroy = _spawnedWalls.Where(x => x.Broken).OrderBy(x => Vector3.Distance(_player.transform.position, x.transform.position)).First();
            slamPos = wallToDestroy.transform.position;
        }
        else
        {
            slamPos = new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z);
        }

        bool hit = false;

        while (timer < _leapDuration)
        {
            horPos = Vector3.Lerp(startPos, slamPos, timer / _leapDuration);

            yPos = Mathf.Lerp(startPos.y, highestPoint, -(timer - _leapDuration) * timer);

            _rb.MovePosition(new Vector3(horPos.x, yPos, horPos.z));

            timer += Time.deltaTime;

            if (!hit)
            {
                if (Physics.CheckCapsule(transform.position, transform.position + new Vector3(0, 4.35f), 1, _playerLayer))
                {
                    _player.KnockBack((_player.transform.position - transform.position).MakeHorizontal(), _leapKnockback);
                    _player.TakeDamage(_leapDamage);

                    hit = true;
                }
            }

            yield return null;
        }

        if (!hit)
        {
            if (Physics.CheckCapsule(transform.position, transform.position + new Vector3(0, 4.35f), 1.5f, _playerLayer))
            {
                _player.KnockBack((_player.transform.position - transform.position).MakeHorizontal(), _leapKnockback);
                _player.TakeDamage(_leapDamage);
            }
        }

        if (wallToDestroy != null)
        {
            var wallRadius = wallToDestroy.Radius * 0.9f;
            var baseAngle = 360 / _leapShardAmount;
            var basePos = wallToDestroy.transform.position;

            wallToDestroy.Die();

            for (int i = 0; i < _leapShardAmount; i++)
            {
                var shard = Instantiate(_shardPrefab, basePos, Quaternion.Euler(new Vector3(0, baseAngle * i + Mathf.Lerp(0, baseAngle, Random.value))));
                shard.transform.position += shard.transform.forward * wallRadius;
                shard.speed = _shardSpeed;
                shard.damage = _shardDamage;
            }
        }

        _rb.isKinematic = false;
        ChangeAudio(stomp);
        prenderCaidaPiedras(true);
        _anim.SetBool("IsStomp", false);
        yield return new WaitForSeconds(_leapRecovery);
        _takingAction = false;
    }

    public override void TakeDamage(float amount, bool bypassCooldown = false)
    {
        StartCoroutine(oneShotTiroPiedras());
        base.TakeDamage(amount);
        Debug.Log(_hp);
        UIManager.instance.UpdateBar(UIManager.Bar.BossHp, _hp);
    }

    public override void Die()
    {
        StopAllCoroutines();
        _anim.SetTrigger("Dead");
        UIManager.instance.ToggleBossBar(false);
        UIManager.instance.HideUI(true);
        _player.Inputs.inputUpdate = _player.Inputs.Nothing;
        _outroTimeline.Play();

        //prenderCaidaPiedras(true);
        //CineManager.instance.PlayAnimation(CineManager.cineEnum.obsidianDead);
        //Destroy(gameObject);
    }
    public void ChangeAudio(AudioClip clip)
    {
        _myAS.clip = clip;
        _myAS.PlayOneShot(_myAS.clip);
    }
    public void prenderTornado(bool prendo)
    {
        tornadoPiedras.SetActive(prendo);
    }
    public void prenderCaidaPiedras(bool prendo)
    {
        caidaPiedras.SetActive(prendo);
    }
    public IEnumerator oneShotTiroPiedras()
    {
        yield return new WaitForSeconds(0.5f);
        prenderCaidaPiedras(false);
    }
}
