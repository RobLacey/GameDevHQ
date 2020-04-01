using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFireControl : MonoBehaviour, IKillable
{
    [SerializeField] float _fireRateMin= default;
    [SerializeField] float _fireRateMax = default;
    [SerializeField] Vector3 _laserOffset = default;
    [SerializeField] AudioClip _laserSFX;
    [SerializeField] float _volume;
    [SerializeField] GameObject _weaponPrefab = default;
    [SerializeField] EventManager _Event_PlayerDead = default;

    //Variables
    float _fireRate = 0;
    AudioSource _audioSource;
    bool _allowFiring = true;
    PoolingAgent _poolingAgent;

    private void Awake()
    {
        _poolingAgent = _poolingAgent.SetUpPoolingAgent(GetComponents<PoolingAgent>(), PoolingID.Weapons);
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _laserSFX;
        _audioSource.volume = _volume;
    }

    private void OnEnable()
    {
        _allowFiring = true;
        _Event_PlayerDead.AddListener(() => StopFiring(), this);
        _fireRate = UnityEngine.Random.Range(_fireRateMin, _fireRateMax);
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        while (_allowFiring)
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

    private void StopFiring()
    {
        _allowFiring = false;
    }
}
