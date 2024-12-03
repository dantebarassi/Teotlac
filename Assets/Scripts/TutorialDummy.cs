using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDummy : Boss
{
    public enum PossibleAttacks
    {
        None,
        Basic,
        Finisher,
        StarCollision,
    }

    [SerializeField] TutorialManager _tutorial;
    [SerializeField] Transform[] _waypoints;
    [SerializeField] Transform _projectileSpawnPos, _floorPos;
    [SerializeField] Projectile _projectile;
    [SerializeField] FloorBurn _floorBurn;
    [SerializeField] float _speed, _shootCooldown, _floorBurnCooldown, _basicDamage, _finisherDamage, _explosionDamage;

    [HideInInspector] public System.Action currentAction;
    [HideInInspector] public PossibleAttacks checkingFor;

    Animator _anim;
    float _currentShootCD, _currentFloorBurnCD;
    int _currentWaypoint = 0;
    bool _orientation = true;

    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    void Update()
    {
        transform.LookAt(GameManager.instance.player.transform);

        currentAction();
    }

    public void Nothing() { }

    public void Moving()
    {
        transform.position = Vector3.MoveTowards(transform.position, _waypoints[_currentWaypoint].position, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _waypoints[_currentWaypoint].position) <= 0.1f)
        {
            _currentWaypoint++;

            if (_currentWaypoint >= _waypoints.Length) _currentWaypoint = 0;
        }
    }

    public void Shooting()
    {
        if (_currentShootCD <= 0)
        {
            //_anim.SetTrigger("Shoot");
            Shoot();

            _currentShootCD = _shootCooldown;
        }
        else
        {
            _currentShootCD -= Time.deltaTime;
        }
    }

    public void Shoot()
    {
        var projectile = Instantiate(_projectile, _projectileSpawnPos.position, Quaternion.identity);
        projectile.transform.forward = GameManager.instance.player.target.position - projectile.transform.position;
        projectile.speed = 20;
        projectile.damage = 0;
    }

    public void MovingAndShooting()
    {
        Moving();

        Shooting();
    }

    public void BurningFloor()
    {
        if (_currentFloorBurnCD <= 0)
        {
            var floorBurn = Instantiate(_floorBurn, new Vector3(transform.position.x, _floorPos.position.y, transform.position.z), Quaternion.identity);
            floorBurn.clockwise = _orientation;
            _orientation = !_orientation;
            _currentFloorBurnCD = _floorBurnCooldown;
        }
        else
        {
            _currentFloorBurnCD -= Time.deltaTime;
        }
    }

    public override void TakeDamage(float amount, bool bypassCooldown = false)
    {
        Debug.Log("Took Damage");

        switch (checkingFor)
        {
            case PossibleAttacks.None:
                Debug.Log("None");
                break;
            case PossibleAttacks.Basic:
                Debug.Log("Is it basic?");
                if (amount == _basicDamage) _tutorial.SuccessfulBasic();
                break;
            case PossibleAttacks.Finisher:
                Debug.Log("Is it finisher?");
                if (amount == _finisherDamage) _tutorial.SuccessfulFinisher();
                break;
            case PossibleAttacks.StarCollision:
                Debug.Log("Is it star?");
                if (amount == _explosionDamage) _tutorial.SuccessfulExplosion();
                break;
            default:
                break;
        }
    }

    public override void Die()
    {
        gameObject.SetActive(false);
    }
}
