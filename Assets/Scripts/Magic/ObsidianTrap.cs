using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsidianTrap : MonoBehaviour, IDamageable
{
    [SerializeField] PlayerProjectile _shardPrefab;
    [SerializeField] int _shardAmount;
    [SerializeField] float _shardDuration, _lowSpawnAngle, _highSpawnAngle;
    [HideInInspector] public float shardSpeed, shardDamage;

    public void TakeDamage(float amount, bool bypassCooldown = false)
    {
        Debug.Log("hit");
        Die();
    }

    public void Die()
    {
        GetComponent<Collider>().enabled = false;

        var halvedAmount = _shardAmount * 0.5f;
        var angleOffset = 360 / halvedAmount;
        var lowBaseEulerRotation = transform.rotation.eulerAngles;
        var highBaseEulerRotation = lowBaseEulerRotation + new Vector3(0, angleOffset * 0.5f);

        for (int i = 0; i < _shardAmount; i++)
        {
            float xRotation, yRotation;

            if (i < halvedAmount)
            {
                xRotation = _lowSpawnAngle;
                yRotation = lowBaseEulerRotation.y + i * angleOffset;
            }
            else
            {
                xRotation = _highSpawnAngle;
                yRotation = highBaseEulerRotation.y + (i - halvedAmount) * angleOffset;
            }

            var shard = Instantiate(_shardPrefab, transform.position, Quaternion.Euler(xRotation, yRotation, 0));
            shard.SetupStats(shardDamage, shardSpeed, _shardDuration);
            shard.transform.localScale *= Random.Range(0.9f, 1.1f);
        }
        
        var lastShard = Instantiate(_shardPrefab, transform.position, Quaternion.identity);
        lastShard.SetupStats(shardDamage, shardSpeed, _shardDuration);
        lastShard.transform.localScale *= Random.Range(0.9f, 1.1f);
        lastShard.transform.forward = transform.up;

        Destroy(gameObject);
    }
}
