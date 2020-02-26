using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsSystem : MonoBehaviour
{
    [SerializeField] GameObject _shields = default;
    [SerializeField] GameObject _spawnBuffer = default;
    [SerializeField] bool _isSpeedBoostActive = false;
    [SerializeField] bool _areShieldsActive = false;
    [SerializeField] float _speedBoost = 10f;
    [SerializeField] float _normalSpeed = 5f;
    [SerializeField] float _SpeedBoostTimer = 0f;
    [SerializeField] float _speedBoostPresence = 7f;
    [SerializeField] AudioClip _powerUpEndSFX = default;
    [SerializeField] GameObject _speedBoostUI = default;
    [SerializeField] GameObject _shieldsUI = default;
    [SerializeField] Weapon _singleShot;
    [SerializeField] Weapon _tripleShot;
    [SerializeField] PowerUpTypes _currentWeapon;

    Action<PowerUpTypes> setActive;

    //Variables
    AudioSource _audioSource;
    Player _myPlayer;
    Collider2D _shieldsCollider;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _myPlayer = GetComponent<Player>();
        _shieldsCollider = GetComponent<CircleCollider2D>();
        _singleShot.SetUp();
        _tripleShot.SetUp();
    }

    private void OnEnable()
    {
        setActive += _singleShot.SetActive;
        setActive += _tripleShot.SetActive;
    }

    private void OnDisable()
    {
        setActive -= _singleShot.SetActive;
        setActive -= _tripleShot.SetActive;
    }

    void Start()
    {
        _audioSource.clip = _singleShot._weaponSFX;
        _shields.SetActive(false);
        _shieldsCollider.enabled = false;
        _speedBoostUI.SetActive(false);
        _shieldsUI.SetActive(false);
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
        if(_singleShot._isActive)
        {
            Vector3 newPosition = transform.position + _singleShot._lazerPositionOffset;
            Instantiate(_singleShot._weaponPrefab, newPosition, Quaternion.identity, _spawnBuffer.transform);
        }
        if (_tripleShot._isActive)
        {
            Instantiate(_tripleShot._weaponPrefab, transform.position, Quaternion.identity, _spawnBuffer.transform);
        }
        _audioSource.Play();
    }

    public void ActivatePowerUp(PowerUpTypes newPowerUp)
    {
        switch (newPowerUp)
        {
            case PowerUpTypes.SingleShot:
                setActive.Invoke(newPowerUp);
                _audioSource.volume = _singleShot._volume;
                _audioSource.clip = _singleShot._weaponSFX;
                _currentWeapon = PowerUpTypes.SingleShot;
                break;
            case PowerUpTypes.TripleShot:
                if (_tripleShot._isActive == true)
                {
                    _tripleShot._timer = _tripleShot._presenece;
                    return;
                }
                setActive.Invoke(newPowerUp);
                _audioSource.volume = _tripleShot._volume;
                _tripleShot._timer = _tripleShot._presenece;
                _audioSource.clip = _tripleShot._weaponSFX;
                _currentWeapon = PowerUpTypes.TripleShot;
                break;
            case PowerUpTypes.SpeedBoost:
                if (_isSpeedBoostActive == true) 
                {
                    _SpeedBoostTimer = _speedBoostPresence;
                    return;
                }
                _speedBoostUI.SetActive(true);
                _SpeedBoostTimer = _speedBoostPresence;
                _isSpeedBoostActive = true;
                _myPlayer.SetSpeed(_speedBoost);
                break;
            case PowerUpTypes.Shield:
                if (_areShieldsActive == true) return;

                _shieldsUI.SetActive(true);
                _areShieldsActive = true;
                _shieldsCollider.enabled = true;
                _shields.SetActive(true);
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
                _speedBoostUI.SetActive(false);
                _isSpeedBoostActive = false;
                _myPlayer.SetSpeed(_normalSpeed);
                break;
            case PowerUpTypes.Shield:
                _shieldsUI.SetActive(false);
                _areShieldsActive = false;
                _shieldsCollider.enabled = false;
                _shields.SetActive(false);
                break;
            default:
                break;
        }
        AudioSource.PlayClipAtPoint(_powerUpEndSFX, Camera.main.transform.position);

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

    public bool ReturnShieldActive()
    {
        return _areShieldsActive;
    }

    public float ReturnFireRate()
    {
        switch (_currentWeapon) 
        {
            case PowerUpTypes.SingleShot:
                return _singleShot._fireRate;
            case PowerUpTypes.TripleShot:
                return _tripleShot._fireRate;
        }
        Debug.Log("No weapon found");
        return 0;
    }
}
