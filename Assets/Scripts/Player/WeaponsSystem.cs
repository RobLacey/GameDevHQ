using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    [SerializeField] AudioClip _powerUpEndSFX = default;
    [SerializeField] float _powerUpEndVolume = default;
    [SerializeField] WeaponSpec _defaultWeapon;
    [SerializeField] EventManager _Event_ActivatePowerUp;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_NewWeapon;
    [SerializeField] EventManager _Event_UpdateWeaponUI;
    [SerializeField] EventManager _Event_CountDownTimer;

    //Variables
    AudioSource _audioSource;
    float _timer = 0;
    WeaponSpec _currentWeapon;
    bool _startTimer = false;
    float _canFireTimer = 0;
    PoolingAgent _poolingAgent;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _poolingAgent = _poolingAgent.SetUpPoolingAgent(GetComponents<PoolingAgent>(), PoolingID.Weapons);
    }

    private void OnEnable()
    {
        ActivateNewWeapon(_defaultWeapon);
    }

    private void Start()
    {
        _Event_NewWeapon.AddListener((x) => ActivateNewWeapon(x));
        _Event_ActivatePowerUp.AddListener(x => ActivateSpeedBoost(x));
        _Event_DeactivatePowerUp.AddListener(x => EndSpeedBoost(x));
    }

    private void Update()
    {
        Fire();

        if (_startTimer)
        { 
            WeaponTimer(); 
        }
    }

    private void Fire() 
    {
        if (Input.GetKey(KeyCode.Space) && Time.time > _canFireTimer)
        {
            _canFireTimer = Time.time + _currentWeapon.FireRate();
            _poolingAgent.InstantiateFromPool(_currentWeapon._weaponPrefab, transform.position, Quaternion.identity);
            _audioSource.Play();
        }

    }

    private void ActivateNewWeapon(object newWeapon)
    {
        _currentWeapon = (WeaponSpec)newWeapon;
        _audioSource.volume = _currentWeapon._volume;
        _audioSource.clip = _currentWeapon._weaponSFX;
        _Event_UpdateWeaponUI.Invoke(_currentWeapon._weaponType);

        if (_currentWeapon._presenece != 0)
        {
            _timer = _currentWeapon._presenece;
            _startTimer = true;
        }
    }

    private void ActivateSpeedBoost(object newPowerUp)
    {
        if ((PowerUpTypes)newPowerUp == PowerUpTypes.SpeedBoost)
        {
            _currentWeapon.ActivateSpeedBoost(true);
        }
    }

    private void EndSpeedBoost(object newPowerUp)
    {
        if ((PowerUpTypes)newPowerUp == PowerUpTypes.SpeedBoost)
        {
            _currentWeapon.ActivateSpeedBoost(false);
        }
    }

    private void WeaponTimer()
    {
        _timer -= Time.deltaTime;
        _Event_CountDownTimer.Invoke(_timer);
        if (_timer <= 0)
        {
            ActivateNewWeapon(_defaultWeapon);
            _audioSource.PlayOneShot(_powerUpEndSFX, _powerUpEndVolume);
            _startTimer = false;
        }
    }
}
