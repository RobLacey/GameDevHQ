using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireControl : MonoBehaviour, IDamageable
{
    [SerializeField] float _fireRateMin= default;
    [SerializeField] float _fireRateMax = default;
    [SerializeField] GameObject _weaponPrefab = default;
    [SerializeField] Vector3 _laserOffset = default;
    [SerializeField] PoolingAgent _poolingAgent;


    //Variables
    float _fireRate = 0;

    private void Awake()
    {
        _poolingAgent = GetComponent<PoolingAgent>();
    }

    private void Start()
    {
        _fireRate = UnityEngine.Random.Range(_fireRateMin, _fireRateMax);
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        GameObject shot;

        while (true)
        {
            _fireRate = UnityEngine.Random.Range(_fireRateMin, _fireRateMax);
            yield return new WaitForSeconds(_fireRate);
            shot = _poolingAgent.InstantiateFromPool(_weaponPrefab, transform.position + _laserOffset);
            shot.GetComponent<ITagable>().SetTagName = gameObject.tag;
        }
    }

    public void Damage(int damage)
    {
        StopAllCoroutines();
    }
}
