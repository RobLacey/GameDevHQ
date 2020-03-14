using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireControl : MonoBehaviour, IKillable
{
    [SerializeField] float _fireRateMin= default;
    [SerializeField] float _fireRateMax = default;
    [SerializeField] GameObject _weaponPrefab = default;
    [SerializeField] Vector3 _laserOffset = default;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] AudioClip _laserSFX;
    [SerializeField] float _volume;

    //Variables
    float _fireRate = 0;
    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _laserSFX;
        _audioSource.volume = _volume;
    }

    private void OnEnable()
    {
        _fireRate = UnityEngine.Random.Range(_fireRateMin, _fireRateMax);
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        while (true)
        {
            yield return new WaitForSeconds(_fireRate);
            _poolingAgent.InstantiateFromPool(_weaponPrefab, transform.position + _laserOffset, Quaternion.identity);
            _audioSource.Play();
            _fireRate = UnityEngine.Random.Range(_fireRateMin, _fireRateMax);
        }
    }

    public void I_Dead(bool collsionKill)
    {
        StopAllCoroutines();
    }
}
