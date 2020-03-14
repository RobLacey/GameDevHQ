using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Weapon
{
    [SerializeField] public string _name;
    [SerializeField] public PowerUpTypes _weapon;
    [SerializeField] public GameObject _weaponPrefab;
    [SerializeField] public bool _isActive;
    [SerializeField] public float _fireRate;
    [SerializeField] float _fireRateMulitpiler = 1f;
    [SerializeField] public float _timer;
    [SerializeField] public float _presenece;
    [SerializeField] public AudioClip _weaponSFX = default;
    [SerializeField] public float _volume = default;

    bool _speedBoostActive;

    public void SetUp()
    {
        _isActive = false;
    }

    public void SetActive(PowerUpTypes activeWeapon)
    {
        if (activeWeapon == _weapon)
        {
            _isActive = true;
        }             
        else
        {
            _isActive = false;
        }
    }

    public void ActivateSpeedBoost(bool active)
    {
        _speedBoostActive = active;
    }

    public float FireRate()
    {
        if (_speedBoostActive)
        {
            return _fireRate / _fireRateMulitpiler;
        }
        return _fireRate;
    }

}
