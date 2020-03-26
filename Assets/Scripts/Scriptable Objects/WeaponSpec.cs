using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Weapon Spec", menuName = "New Weapon Spec")]

public class WeaponSpec: ScriptableObject
{
    [SerializeField] public PowerUpTypes _weaponType;
    [SerializeField] public GameObject _weaponPrefab;
    [SerializeField] public float _fireRate = 2f;
    [SerializeField] float _fireRateMulitpiler = 1.5f;
    [SerializeField] public float _presenece;
    [SerializeField] public AudioClip _weaponSFX = default;
    [SerializeField] public float _volume = 0.7f;

    //Variables
    bool _speedBoostActive;

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
