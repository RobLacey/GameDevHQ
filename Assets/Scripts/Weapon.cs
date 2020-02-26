using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Weapon
{
    [SerializeField] public PowerUpTypes _weapon;
    [SerializeField] public GameObject _weaponPrefab;
    [SerializeField] public bool _isActive;
    [SerializeField] public float _timer;
    [SerializeField] public float _presenece;
    [SerializeField] public AudioClip _weaponSFX = default;
    [SerializeField] public float _volume = default;
    [SerializeField] public GameObject _weaponUIPrefab = default;
    [SerializeField] public float _fireRate;
    [SerializeField] public Vector3 _laserOffset = default;

    public void SetUp()
    {
        _isActive = false;
        _weaponUIPrefab.SetActive(false);
    }

    public void SetActive(PowerUpTypes activeWeapon)
    {
        if (activeWeapon == _weapon)
        {
            _isActive = true;
            _weaponUIPrefab.SetActive(true);
        }             
        else
        {
            _isActive = false;
            _weaponUIPrefab.SetActive(false);
        }
    }

}
