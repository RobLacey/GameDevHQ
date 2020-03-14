using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsSystem : MonoBehaviour, IWeaponSystem
{
    [SerializeField] AudioClip _powerUpEndSFX = default;
    [SerializeField] Weapon _currentWeapon = default;
    [SerializeField] EventManager _Event_ActivatePowerUp;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_DefaultWeapon;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] Weapon[] _weapons;

    Action<PowerUpTypes> setActive;

    //Variables
    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        foreach (var weapon in _weapons)
        {
            setActive += weapon.SetActive;
        }
    }

    private void OnDisable()
    {
        foreach (var weapon in _weapons)
        {
            setActive -= weapon.SetActive;
        }
    }

    private void Start()
    {
        foreach (var weapon in _weapons)
        {
            weapon.SetUp();
        }

        _Event_ActivatePowerUp.AddListener(x => I_ActivateWeapon(x));
        _Event_DeactivatePowerUp.AddListener(x => I_DeactivateWeapon(x));
        I_ActivateWeapon(PowerUpTypes.SingleShot);
    }

    void Update()
    {
        if (_currentWeapon._isActive && _currentWeapon._weapon != PowerUpTypes.SingleShot)
        { 
            WeaponTimer(); 
        }
    }

    public void I_Fire() 
    {
        _poolingAgent.InstantiateFromPool(_currentWeapon._weaponPrefab, transform.position, Quaternion.identity);
        _audioSource.Play();
    }

    public void I_ActivateWeapon(object newPowerUp)
    {
        PowerUpTypes newWeapon = (PowerUpTypes)newPowerUp;

        foreach (var weapon in _weapons)
        {
            if (weapon._weapon == newWeapon)
            {
                _currentWeapon = weapon;
                SetUpWeapon();
                break;
            }
        }

        if (newWeapon == PowerUpTypes.SpeedBoost)
        {
            SetUpSpeedBoost(true);
        }
    }

    public void I_DeactivateWeapon(object oldPowerUp)
    {
        switch ((PowerUpTypes)oldPowerUp)
        {
            case PowerUpTypes.SpeedBoost:
                SetUpSpeedBoost(false);
                break;
        }
    }

    private void SetUpWeapon()
    {
        setActive.Invoke(_currentWeapon._weapon);
        _audioSource.volume = _currentWeapon._volume;
        _audioSource.clip = _currentWeapon._weaponSFX;

        if (_currentWeapon._weapon != PowerUpTypes.SingleShot)
        {
            _currentWeapon._timer = _currentWeapon._presenece;
        }
    }

    private void SetUpSpeedBoost(bool active)
    {
        foreach (var item in _weapons)
        {
            item.ActivateSpeedBoost(active);
        }
    }

    private void WeaponTimer()
    {
        _currentWeapon._timer -= Time.deltaTime;
        if (_currentWeapon._timer <= 0)
        {
            _Event_DefaultWeapon.Invoke(PowerUpTypes.SingleShot);
            I_ActivateWeapon(PowerUpTypes.SingleShot);
            AudioSource.PlayClipAtPoint(_powerUpEndSFX, Camera.main.transform.position);
        }
    }

    public float I_ReturnFireRate()
    {
        foreach (var item in _weapons)
        {
            if (item._weapon == _currentWeapon._weapon)
            {
                return item.FireRate();
            }
        }        
        Debug.Log("No weapon found");
        return 0;
    }
}
