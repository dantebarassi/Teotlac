using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : Boss
{
    [SerializeField] PlayerController _player;
    [SerializeField] Projectile _projectile;
    [SerializeField] Transform _projectileSpawnPos;
    [SerializeField] float _moveDuration, _attackDuration, _attackInterval, _speed;

    Vector3 _moveDir;
    float _timer = Mathf.Infinity, _attackCooldown = 0;
    bool _move = false;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.forward = (_player.transform.position - transform.position).normalized;
        _timer += Time.deltaTime;

        if (_timer < _moveDuration)
        {
            _move = true;
        }
        else if (_timer < _moveDuration + _attackDuration)
        {
            _move = false;
            Attack();
        }
        else
        {
            _moveDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            _timer = 0;
        }
    }

    private void FixedUpdate()
    {
        if (_move) _rb.MovePosition(transform.position + _moveDir * _speed * Time.fixedDeltaTime);
    }

    public void Attack()
    {
        if (_attackCooldown <= 0)
        {
            var projectile = Instantiate(_projectile, _projectileSpawnPos.position, Quaternion.identity);
            projectile.transform.forward = _player.transform.position + Vector3.up - projectile.transform.position;
            projectile.speed = 20;
            projectile.damage = 0;
            _attackCooldown = _attackInterval;
        }
        else
        {
            _attackCooldown -= Time.deltaTime;
        }
    }
}
