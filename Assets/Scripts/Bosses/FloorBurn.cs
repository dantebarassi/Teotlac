using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorBurn : MonoBehaviour
{
    [SerializeField] float _lifetime, _rotationSpeed, _growRate;
    public bool clockwise;

    private void Start()
    {
        if (!clockwise) _rotationSpeed *= -1;
    }

    void Update()
    {
        _lifetime -= Time.deltaTime;

        if (_lifetime <= 0) Destroy(gameObject);

        transform.rotation = Quaternion.AngleAxis(_rotationSpeed * Time.deltaTime, Vector3.up) * transform.rotation;
        transform.localScale += _growRate * Time.deltaTime * Vector3.one;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage(0);
        }
    }
}
