using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SteeringAgent : Entity
{
    //protected Pathfinding _pf;
    //protected GameManager gm { get => GameManager.instance; }

    [SerializeField] protected float _maxSpeed, _maxForce, _obstacleViewRadius, _viewRadius, _viewAngle;

    //[SerializeField] protected LayerMask _obstacleLM;

    protected Vector3 _velocity;
    public Vector3 Velocity { get { return _velocity; } }

    protected void Move()
    {
        _rb.MovePosition(transform.position + _velocity * Time.deltaTime);
        if (_velocity != Vector3.zero) _rb.MoveRotation(Quaternion.LookRotation(_velocity));
    }

    protected Vector3 Pursuit(SteeringAgent agent)
    {
        Vector3 futurePos = agent.transform.position + agent.Velocity;
        return Seek(futurePos);
    }

    protected Vector3 Evade(SteeringAgent agent)
    {
        return -Pursuit(agent);
    }

    protected Vector3 Arrive(Vector3 targetPos)
    {
        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist <= _viewRadius)
        {
            Vector3 desired = (targetPos - transform.position).normalized;
            desired *= _maxSpeed * (dist / _viewRadius);
            return CalculateSteering(desired);
        }

        return Seek(targetPos);
    }

    //protected bool ObstacleAvoidance(out Vector3 v)
    //{
    //    bool lRaycast = Physics.Raycast(transform.position + transform.up * 0.5f, transform.right, _obstacleViewRadius, _obstacleLM);
    //    bool rRaycast = Physics.Raycast(transform.position - transform.up * 0.5f, transform.right, _obstacleViewRadius, _obstacleLM);
    //
    //    if (lRaycast)
    //    {
    //        v = CalculateSteering(-transform.up * _maxSpeed);
    //        return true;
    //    }
    //    else if (rRaycast)
    //    {
    //        v = CalculateSteering(transform.up * _maxSpeed);
    //        return true;
    //    }
    //
    //    v = Vector3.zero;
    //    return false;
    //}

    protected Vector3 Seek(Vector3 targetPos)
    {
        Vector3 desired = targetPos - transform.position;
        desired.Normalize();
        desired *= _maxSpeed;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce * Time.deltaTime);

        return steering;
    }

    //public void SeekAvoidingObstacles(Vector3 targetPos)
    //{
    //    if (ObstacleAvoidance(out Vector3 obs))
    //    {
    //        AddForce(obs);
    //    }
    //    else
    //    {
    //        AddForce(Seek(targetPos));
    //    }
    //
    //    Move();
    //}

    protected Vector3 Flee(Vector3 targetPos)
    {
        return -Seek(targetPos);
    }

    protected Vector3 CalculateSteering(Vector3 desired)
    {
        return Vector3.ClampMagnitude(desired - _velocity, _maxForce * Time.deltaTime);
    }

    protected void AddForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxSpeed);
    }

    protected void Stop()
    {
        _velocity = Vector3.zero;
    }

    //protected void UpdatePath(Vector3 targetPos)
    //{
    //    if (transform.position.InLineOfSightOf(targetPos, gm.wallLayer))
    //    {
    //        _pathToFollow.Clear();
    //        _pathToFollow.Add(targetPos);
    //    }
    //    else
    //    {
    //        _pathToFollow = _pf.ThetaStar(transform.position, targetPos, gm.wallLayer);
    //    }
    //}
    //
    //protected bool TravelThroughPath()
    //{
    //    if (_pathToFollow == null || _pathToFollow.Count == 0) return true;
    //
    //    if (!transform.position.InLineOfSightOf(_pathToFollow[0], gm.wallLayer))
    //    {
    //        UpdatePath(_pathToFollow.Last());
    //    }
    //
    //    SeekAvoidingObstacles(_pathToFollow[0]);
    //
    //    if (Vector3.Distance(transform.position, _pathToFollow[0]) < 0.05f)
    //    {
    //        _pathToFollow.RemoveAt(0);
    //    }
    //
    //    return false;
    //}

    protected void Aim(Vector3 target)
    {
        _rb.MoveRotation(Quaternion.LookRotation(Seek(target)));
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var rCenter = transform.position + transform.up * 0.5f;
        var lCenter = transform.position - transform.up * 0.5f;

        Gizmos.DrawLine(rCenter, rCenter + transform.right * _obstacleViewRadius);
        Gizmos.DrawLine(lCenter, lCenter + transform.right * _obstacleViewRadius);
        Gizmos.DrawLine(rCenter + transform.right * _obstacleViewRadius, lCenter + transform.right * _obstacleViewRadius);

        Gizmos.DrawWireSphere(transform.position, _viewRadius);

        Vector3 dirA = GetDirFromAngle(_viewAngle * 0.5f);
        Vector3 dirB = GetDirFromAngle(-_viewAngle * 0.5f);

        Gizmos.DrawLine(transform.position, transform.position + dirA.normalized * _viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + dirB.normalized * _viewRadius);
    }

    public Vector3 GetDirFromAngle(float angleInDegrees)
    {
        float angle = angleInDegrees + transform.eulerAngles.z;
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
    }

    public override void Die()
    {
        Destroy(gameObject);
    }
}
