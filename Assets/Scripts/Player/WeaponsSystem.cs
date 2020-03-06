using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsSystem : MonoBehaviour, IWeaponSystem
{
    [SerializeField] GameObject _shields = default;
    [SerializeField] bool _isSpeedBoostActive = false;
    [SerializeField] bool _areShieldsActive = false;
    [SerializeField] float _speedBoost = 10f;
    [SerializeField] float _normalSpeed = 5f;
    [SerializeField] float _SpeedBoostTimer = 0f;
    [SerializeField] float _speedBoostPresence = 7f;
    [SerializeField] AudioClip _powerUpEndSFX = default;
    [SerializeField] Weapon _singleShot = default;
    [SerializeField] Weapon _tripleShot = default;
    [SerializeField] Weapon _sideShot = default;
    [SerializeField] PowerUpTypes _currentWeapon = default;
    [SerializeField] EventManager _Event_ActivatePowerUp;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_AddHealth;
    [SerializeField] EventManager _Event_DefaultWeapon;
    [SerializeField] PoolingAgent _poolingAgent;

    Action<PowerUpTypes> setActive;
    GameObject _currentWeaponPrefab;

    //Properties
    public bool ShieldsAreActive 
        { get { return _areShieldsActive; } set { _areShieldsActive = value; } }

    //Variables
    AudioSource _audioSource;
    CircleCollider2D _shieldsCollider;
    ISpeedBoostable _speedBoostable;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _speedBoostable = GetComponent<ISpeedBoostable>();
        _shieldsCollider = GetComponentInChildren<CircleCollider2D>();
        _singleShot.SetUp();
        _tripleShot.SetUp();
        _sideShot.SetUp();
    }

    private void OnEnable()
    {
        setActive += _singleShot.SetActive;
        setActive += _tripleShot.SetActive;
        setActive += _sideShot.SetActive;
        _Event_ActivatePowerUp.AddListener(x => ActivatePowerUp(x));
    }

    private void OnDisable()
    {
        setActive -= _singleShot.SetActive;
        setActive -= _tripleShot.SetActive;
        setActive -= _sideShot.SetActive;
    }

    void Start()
    {
        SetUpShields(false);
        SetUpSpeedBoost(false);
        //_currentWeaponPrefab = _singleShot._weaponPrefab;
        //SetUpWeapon(_singleShot._volume, _singleShot._weaponSFX, _singleShot._weapon);
        ActivatePowerUp(PowerUpTypes.SingleShot);
    }

    void Update()
    {
        if (_tripleShot._isActive) 
        { 
            WeaponTimer(_tripleShot); 
        }

        if (_sideShot._isActive) 
        { 
            WeaponTimer(_sideShot);
        }

        if (_isSpeedBoostActive)
        {
            SpeedBoostTimer();
        }
    }

    public void Fire() 
    {
        _poolingAgent.InstantiateFromPool(_currentWeaponPrefab, transform.position, Quaternion.identity);
        _audioSource.Play();
    }

    public void ActivatePowerUp(object newPowerUp)
    {
        PowerUpTypes weapon = (PowerUpTypes)newPowerUp;
        switch (weapon)
        {
            case PowerUpTypes.SingleShot:
                _Event_DefaultWeapon.Invoke(weapon);
                _currentWeaponPrefab = _singleShot._weaponPrefab;
                SetUpWeapon(_singleShot._volume, _singleShot._weaponSFX, weapon);
                break;
            case PowerUpTypes.TripleShot:
                _currentWeaponPrefab = _tripleShot._weaponPrefab;
                _tripleShot._timer = _tripleShot._presenece;
                SetUpWeapon(_tripleShot._volume, _tripleShot._weaponSFX, weapon);
                break;
            case PowerUpTypes.SideShot:
                _currentWeaponPrefab = _sideShot._weaponPrefab;
                _sideShot._timer = _sideShot._presenece;
                SetUpWeapon(_sideShot._volume, _sideShot._weaponSFX, weapon);
                break;
            case PowerUpTypes.SpeedBoost:
                _SpeedBoostTimer = _speedBoostPresence;
                SetUpSpeedBoost(true);
                break;
            case PowerUpTypes.Shield:
                if (ShieldsAreActive == true) return;
                SetUpShields(true);
                break;
            case PowerUpTypes.Health:
                _Event_AddHealth.Invoke();
                break;
            default:
                break;
        }
    }

    public void DeactivatePowerUps(PowerUpTypes oldPowerUp)
    {
        switch (oldPowerUp)
        {
            case PowerUpTypes.SpeedBoost:
                SetUpSpeedBoost(false);
                break;
            case PowerUpTypes.Shield:
                SetUpShields(false);
                break;
            default:
                break;
        }
        _Event_DeactivatePowerUp.Invoke(oldPowerUp);
        AudioSource.PlayClipAtPoint(_powerUpEndSFX, Camera.main.transform.position);
    }

    private void SetUpWeapon(float volume, AudioClip audioClip, PowerUpTypes weaponType)
    {
        setActive.Invoke(weaponType);
        _audioSource.volume = volume;
        _audioSource.clip = audioClip;
        _currentWeapon = weaponType;
    }

    private void SetUpSpeedBoost(bool active)
    {
        _isSpeedBoostActive = active;
        _singleShot.ActivateSpeedBoost(active);
        _tripleShot.ActivateSpeedBoost(active);
        _sideShot.ActivateSpeedBoost(active);
        if (active)
        {
            _SpeedBoostTimer = _speedBoostPresence;
            _speedBoostable.SetSpeed = _speedBoost;
        }
        else
        {
            _speedBoostable.SetSpeed = _normalSpeed;
        }
    }

    private void SetUpShields(bool active)
    {
        ShieldsAreActive = active;
        _shieldsCollider.enabled = active;
        _shields.SetActive(active);
    }

    private void WeaponTimer(Weapon weapon)
    {
        weapon._timer -= Time.deltaTime;
        if (weapon._timer <= 0)
        {
            ActivatePowerUp(PowerUpTypes.SingleShot);
            AudioSource.PlayClipAtPoint(_powerUpEndSFX, Camera.main.transform.position);
        }
    }

    private void SpeedBoostTimer()
    {
        _SpeedBoostTimer -= Time.deltaTime;
        if (_SpeedBoostTimer <= 0)
        {
            DeactivatePowerUps(PowerUpTypes.SpeedBoost);
        }
    }

    public float ReturnFireRate()
    {
        switch (_currentWeapon) 
        {
            case PowerUpTypes.SingleShot:
                return _singleShot.FireRate();
            case PowerUpTypes.TripleShot:
                return _tripleShot.FireRate();
            case PowerUpTypes.SideShot:
                return _sideShot.FireRate();
        }
        Debug.Log("No weapon found");
        return 0;
    }
}
