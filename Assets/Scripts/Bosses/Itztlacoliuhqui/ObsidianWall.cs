using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsidianWall : MonoBehaviour, IDamageable
{
    public Itztlacoliuhqui boss;

    [SerializeField] GameObject _completeWall, _brokenWall;

    [SerializeField] float _radius, _nodeBlockRadius, _riseTime;

    [SerializeField] bool _unbreakable;

    [SerializeField] ParticleSystem piedritas, piedras, humo;

    AudioSource _myAS;
    public float Radius
    {
        get
        {
            return _radius;
        }
    }

    List<Node> _overlappingNodes = new List<Node>();

    bool _broken = false, _rising = false;

    public bool Broken
    {
        get
        {
            return _broken;
        }
    }

    private void Start()
    {
        _myAS = GetComponent<AudioSource>();

        foreach (var item in ObsidianPathfindManager.instance.allNodes)
        {
            if (Vector3.Distance(transform.position, item.transform.position) <= _nodeBlockRadius)
            {
                _overlappingNodes.Add(item);
                item.SetBlock(true);
            }
        }
    }

    public void Break()
    {
        if (_unbreakable)
        {
            return;
        }
        if (_rising)
        {
            Die();
            return;
        }

        _completeWall.SetActive(false);
        _brokenWall.SetActive(true);
        piedras.Play();
        piedritas.Play();
        humo.Play();
        _myAS.Play();
        _broken = true;
    }

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        if (_broken)
        {
            Die();
        }
        else
        {
            Break();
        }
    }

    public void Die()
    {
        StartCoroutine(Death());
    }

    public IEnumerator Death()
    {
        foreach (var item in _overlappingNodes)
        {
            item.SetBlock(false);
        }

        boss.WallDestroyed(this);
        _myAS.Play();
        piedras.Play();
        piedritas.Play();
        humo.Play();
        _completeWall.SetActive(false);
        _brokenWall.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
    }

    IEnumerator Rise()
    {
        var unburiedPos = _completeWall.transform.localPosition;
        var buriedPos = new Vector3(_completeWall.transform.localPosition.x, -_completeWall.transform.localPosition.y, _completeWall.transform.localPosition.z);

        _completeWall.transform.position = buriedPos;

        float timer = 0;

        while (timer < _riseTime)
        {
            _completeWall.transform.localPosition = Vector3.Lerp(buriedPos, unburiedPos, timer / _riseTime);

            timer += Time.deltaTime;

            yield return null;
        }

        foreach (var item in ObsidianPathfindManager.instance.allNodes)
        {
            if (Vector3.Distance(transform.position, item.transform.position) <= _nodeBlockRadius)
            {
                _overlappingNodes.Add(item);
                item.SetBlock(true);
            }
        }

        _completeWall.transform.localPosition = unburiedPos;

        _rising = false;
    }
}
