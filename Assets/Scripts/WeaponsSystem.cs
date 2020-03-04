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
    //[SerializeField] GameObject _speedBoostUI = default;
    //[SerializeField] GameObject _shieldsUI = default;
    [SerializeField] Weapon _singleShot = default;
    [SerializeField] Weapon _tripleShot = default;
    [SerializeField] PowerUpTypes _currentWeapon = default;
    [SerializeField] EventManager _Event_ActivatePowerUp;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_AddHealth;
    [SerializeField] PoolingAgent _poolingAgent;

    Action<PowerUpTypes> setActive;

    //Properties
    public bool ShieldsAreActive 
        { get { return _areShieldsActive; } set { _areShieldsActive = value; } }

    //Variables
    AudioSource _audioSource;
    Collider2D _shieldsCollider;
    ISpeedBoostable _speedBoostable;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _speedBoostable = GetComponent<ISpeedBoostable>();
        _shieldsCollider = GetComponent<CircleCollider2D>();
        _singleShot.SetUp();
        _tripleShot.SetUp();
    }

    private void OnEnable()
    {
        setActive += _singleShot.SetActive;
        setActive += _tripleShot.SetActive;
        _Event_ActivatePowerUp.AddListener(x => ActivatePowerUp(x));
    }

    private void OnDisable()
    {
        setActive -= _singleShot.SetActive;
        setActive -= _tripleShot.SetActive;
    }

    void Start()
    {
        SetUpShields(false);
        SetUpSpeedBoost(false);
        ActivatePowerUp(PowerUpTypes.SingleShot);
    }

    void Update()
    {
        if (_tripleShot._isActive) 
        { 
            TripleShotTimer(); 
        }

        if (_isSpeedBoostActive)
        {
            SpeedBoostTimer();
        }
    }

    public void Fire()
    {
        GameObject shot = default ;

        if (_singleShot._isActive)
        {
            Vector3 newPosition = transform.position + _singleShot._laserOffset;
            shot = _poolingAgent.InstantiateFromPool(_singleShot._weaponPrefab, newPosition);
        }
        if (_tripleShot._isActive)
        {
            shot = _poolingAgent.InstantiateFromPool(_tripleShot._weaponPrefab, transform.position);
        }
        if (shot != null)
        {
            shot.GetComponent<ITagable>().SetTagName = gameObject.tag;
        }
        _audioSource.Play();
    }

    public void ActivatePowerUp(object newPowerUp)
    {
        PowerUpTypes weapon = (PowerUpTypes)newPowerUp;
        switch (weapon)
        {
            case PowerUpTypes.SingleShot:
                SetUpWeapon(_singleShot._volume, _singleShot._weaponSFX, PowerUpTypes.SingleShot);
                break;
            case PowerUpTypes.TripleShot:
                _tripleShot._timer = _tripleShot._presenece;
                SetUpWeapon(_tripleShot._volume, _tripleShot._weaponSFX, PowerUpTypes.TripleShot);
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
        //_speedBoostUI.SetActive(active);
        _isSpeedBoostActive = active;
        _singleShot.ActivateSpeedBoost(active);
        _tripleShot.ActivateSpeedBoost(active);
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
        //_shieldsUI.SetActive(active);
        ShieldsAreActive = active;
        _shieldsCollider.enabled = active;
        _shields.SetActive(active);
    }

    private void TripleShotTimer()
    {
        _tripleShot._timer -= Time.deltaTime;
        if (_tripleShot._timer <= 0)
        {
            ActivatePowerUp(PowerUpTypes.SingleShot);
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
        }
        Debug.Log("No weapon found");
        return 0;
    }
}
