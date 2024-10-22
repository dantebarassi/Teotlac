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
    [SerializeField] LayerMask _playerLayer, _groundLayer;

    [Header("Shards")]
    [SerializeField] ObsidianShard _shardPrefab;
    [SerializeField] float _shardSpeed, _shardDamage;

    [Header("Buds")]
    [SerializeField] ObsidianBud _budPrefab;
    [SerializeField] int _projectileAmount;
    [SerializeField] float _budActivationRange, _budStrikeDamage;

    [Header("Quick Shot")]
    [SerializeField] Transform _quickShotSpawnPos; // crear un empty hijo del hueso de la muñeca de la mano que no lleva la daga, moverlo cerca de la palma y cargarlo aca
    [SerializeField] int _quickShotProjectileAmount;
    [SerializeField] float _quickShotHorDirVariation, _quickShotVerDirVariation;

    [Header("Ground Spikes")]
    [SerializeField] VisualEffect _spikes;
    [SerializeField] float _spikesBaseOffset, _spikesBaseSizeX, _spikesSizeY, _spikesSizeZ, _spikesSizeGrowthX, _spikesDelay, _spikesDamage, _spikesDuration;
    [SerializeField] int _spikesExtraWaves;

    [Header("Placeholder Wall Spike")]
    [SerializeField] ObsidianWall _wallPrefab;
    [SerializeField] VisualEffect _movingSpikes;
    [SerializeField] float _wallSpikeSpawnOffset, _wallSpikeTravelSpeed, _wallSpawnDelay, _wallSpikeKnockback, _wallSpikeDamage;

    ObjectPool<ObsidianBud> _budPool;
    Factory<ObsidianBud> _budFactory;
    ObjectPool<ObsidianShard> _shardPool;
    Factory<ObsidianShard> _shardFactory;

    List<ObsidianBud> _spawnedBuds = new();
    List<ObsidianBud> _bloomingBuds = new();

    AudioSource _myAS;

    protected override void Awake()
    {
        base.Awake();

        UIManager.instance.UpdateBar(UIManager.Bar.BossHp, _hp, _maxHp);

        _budFactory = new Factory<ObsidianBud>(_budPrefab);
        _budPool = new ObjectPool<ObsidianBud>(_budFactory.GetObject, ObsidianBud.TurnOff, ObsidianBud.TurnOn, 10);

        _shardFactory = new Factory<ObsidianShard>(_shardPrefab);
        _shardPool = new ObjectPool<ObsidianShard>(_shardFactory.GetObject, ObsidianShard.TurnOff, ObsidianShard.TurnOn, 50);

        _spikes.SetFloat("SizeZ", _spikesSizeZ);
        _spikes.SetFloat("Lifetime", _spikesDuration);

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
        //StartCoroutine(BudStrikeTest());
        //StartCoroutine(yea());
        StartCoroutine(Test());
    }
    IEnumerator yea()
    {
        StartCoroutine(GroundSpiking());
        yield return new WaitForSeconds(5);
        StartCoroutine(yea());
    }
    void Update()
    {

    }

    IEnumerator Test()
    {
        QuickShot(_quickShotProjectileAmount);

        yield return new WaitForSeconds(1.5f);

        QuickShot(_quickShotProjectileAmount);

        yield return new WaitForSeconds(1.5f);

        QuickShot(_quickShotProjectileAmount);

        yield return new WaitForSeconds(3);

        _bloomingBuds = _spawnedBuds.Where(x => Vector3.Distance(_player.transform.position, x.transform.position) <= _budActivationRange).ToList();

        if (_bloomingBuds.Any())
        {
            StartCoroutine(BloomTest());
            yield return new WaitForSeconds(1.5f);
        }

        StartCoroutine(Test());
    }

    IEnumerator BudStrikeTest()
    {
        yield return new WaitForSeconds(1.5f);

        BudStrike();

        yield return new WaitForSeconds(1.5f);

        BudStrike();

        yield return new WaitForSeconds(1.5f);

        BudStrike();

        yield return new WaitForSeconds(1.5f);

        _bloomingBuds = _spawnedBuds.Where(x => Vector3.Distance(_player.transform.position, x.transform.position) <= _budActivationRange).ToList();

        if (_bloomingBuds.Any())
        {
            StartCoroutine(BloomTest());
            yield return new WaitForSeconds(1.5f);
        }

        QuickShot(_quickShotProjectileAmount);

        yield return new WaitForSeconds(1.5f);

        QuickShot(_quickShotProjectileAmount);

        yield return new WaitForSeconds(1.5f);

        StartCoroutine(PlaceholderWallSpiking());

        yield return new WaitForSeconds(3);

        if (_bloomingBuds.Any())
        {
            StartCoroutine(BloomTest());
            yield return new WaitForSeconds(1.5f);
        }

        StartCoroutine(GroundSpiking());
        yield return new WaitForSeconds(3f);

        StartCoroutine(BudStrikeTest());
    }

    IEnumerator BloomTest()
    {
        Prebloom();

        yield return new WaitForSeconds(1);

        Bloom();
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
        bud.Initialize(_budPool, _shardPool, BudDestroyed, spawnPos, _budStrikeDamage, _shardSpeed, _shardDamage);

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
        bud.Initialize(_budPool, _shardPool, BudDestroyed, pos, _budStrikeDamage, _shardSpeed, _shardDamage);

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

    public void QuickShot(int projectileCount)
    {
        var dir = _player.target.position - _quickShotSpawnPos.position;

        for (int i = 0; i < projectileCount; i++)
        {
            var shard = _shardPool.Get();
            shard.Initialize(_shardPool, _shardSpeed, _shardDamage, SpawnBud);
            shard.transform.position = _quickShotSpawnPos.position;
            shard.transform.forward = dir.VectorVariation(i, _quickShotHorDirVariation, _quickShotVerDirVariation);
        }
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

    IEnumerator GroundSpiking()
    {
        Vector3 targetPos, nextPos, dir;

        if (_player.Grounded) targetPos = _player.transform.position;
        else
        {
            if (Physics.Raycast(_player.transform.position, Vector3.down, out var hit, Mathf.Infinity, _groundLayer)) targetPos = hit.point;
            else yield break;
        }

        dir = (targetPos - transform.position).MakeHorizontal().normalized;
        transform.forward = dir;
        Quaternion rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        nextPos = transform.position + dir * _spikesBaseOffset;
        dir *= _spikesSizeZ;
        float nextSize = _spikesBaseSizeX, halfSize = _spikesBaseSizeX * 0.5f;
        List<GameObject> allSpikes = new();

        while (Vector3.Distance(nextPos, targetPos) > dir.magnitude)
        {
            var currentsSpikes = Instantiate(_spikes, nextPos, rotation);
            currentsSpikes.SetFloat("Radiu 2", nextSize);
            currentsSpikes.Play();
            allSpikes.Add(currentsSpikes.gameObject);

            StartCoroutine(SpikesHitCheck(nextPos, halfSize, rotation));

            nextPos += dir;
            nextSize += _spikesSizeGrowthX;
            halfSize = nextSize * 0.5f;

            yield return new WaitForSeconds(_spikesDelay);
        }

        for (int i = -1; i < _spikesExtraWaves; i++)
        {
            var currentsSpikes = Instantiate(_spikes, nextPos, rotation);
            currentsSpikes.SetFloat("Radiu 2", nextSize);
            currentsSpikes.Play();
            allSpikes.Add(currentsSpikes.gameObject);

            StartCoroutine(SpikesHitCheck(nextPos, halfSize, rotation));

            nextPos += dir;
            nextSize += _spikesSizeGrowthX;
            halfSize = nextSize * 0.5f;

            yield return new WaitForSeconds(_spikesDelay);
        }

        yield return new WaitForSeconds(5);

        foreach (var item in allSpikes) Destroy(item);
    }

    IEnumerator SpikesHitCheck(Vector3 center, float halfExtents, Quaternion orientation)
    {
        float timer = 0;

        while (timer < _spikesDuration)
        {
            timer += Time.deltaTime;

            if (Physics.CheckBox(center, new Vector3(halfExtents, _spikesSizeY, _spikesSizeZ), orientation, _playerLayer))
            {
                _player.TakeDamage(_spikesDamage);

                yield break;
            }

            yield return null;
        }
    }
}