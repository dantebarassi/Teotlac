using IA2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;

public class NewItztlacoliuhqui : Boss
{
    public enum Actions
    {
        Inactive,
        Approach,
        Melee,
        QuickShot,
        PowerShot,
        BudStrike,
        Bloom,
        ThornTrap,
        Limb,
        Garden
    }

    EventFSM<Actions> _fsm;
    Pathfinding _pf;

    [SerializeField] bool _playOnStart;
    [SerializeField] PlayerController _player;
    [SerializeField] LayerMask _playerLayer;

    [Header("Buds")]
    [SerializeField] ObsidianBud _budPrefab;
    [SerializeField] int _projectileAmount;
    [SerializeField] float _budActivationRange, _budStrikeDamage;

    ObjectPool<ObsidianBud> _budPool;
    Factory<ObsidianBud> _budFactory;

    List<ObsidianBud> _spawnedBuds = new();
    List<ObsidianBud> _bloomingBuds = new();

    AudioSource _myAS;

    protected override void Awake()
    {
        base.Awake();

        UIManager.instance.UpdateBar(UIManager.Bar.BossHp, _hp, _maxHp);

        _budFactory = new Factory<ObsidianBud>(_budPrefab);
        _budPool = new ObjectPool<ObsidianBud>(_budFactory.GetObject, ObsidianBud.TurnOff, ObsidianBud.TurnOn, 10);

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
        var melee = new State<Actions>("Melee");
        var quickShot = new State<Actions>("QuickShot");
        var powerShot = new State<Actions>("PowerShot");
        var budStrike = new State<Actions>("BudStrike");
        var bloom = new State<Actions>("Bloom");
        var thornTrap = new State<Actions>("ThornTrap");
        var limb = new State<Actions>("Limb");
        var garden = new State<Actions>("Garden");

        StateConfigurer.Create(inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(approach)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(melee)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(quickShot)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(powerShot)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(budStrike)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(bloom)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(thornTrap)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(limb)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .SetTransition(Actions.Garden, garden)
            .Done();

        StateConfigurer.Create(garden)
            .SetTransition(Actions.Inactive, inactive)
            .SetTransition(Actions.Approach, approach)
            .SetTransition(Actions.Melee, melee)
            .SetTransition(Actions.QuickShot, quickShot)
            .SetTransition(Actions.PowerShot, powerShot)
            .SetTransition(Actions.BudStrike, budStrike)
            .SetTransition(Actions.Bloom, bloom)
            .SetTransition(Actions.ThornTrap, thornTrap)
            .SetTransition(Actions.Limb, limb)
            .Done();

        #endregion

        #region FSM State Setup

        budStrike.OnEnter += x =>
        {
            // empezar animacion
        };

        bloom.OnEnter += x =>
        {
            // empezar animacion
        };

        #endregion
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void BudStrike()
    {
        Vector3 spawnPos = _player.transform.position;

        var bud = _budPool.Get();
        bud.transform.position = spawnPos;

        if (Physics.CheckCapsule(spawnPos, spawnPos + Vector3.up, 0.1f, _playerLayer))
        {
            //_player.KnockBack(_player.transform.position - nextSpawnPos, _wallSpikeKnockback);
            _player.TakeDamage(_budStrikeDamage);
        }
    }

    public void Prebloom()
    {
        _bloomingBuds = _spawnedBuds.Where(x => Vector3.Distance(_player.transform.position, x.transform.position) <= _budActivationRange).ToList();

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
            item.Bloom(_player.transform.position, _projectileAmount);
        }
    }
}