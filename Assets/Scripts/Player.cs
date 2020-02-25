using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour, IDamageable
{
    //TODO resetting Timers when multiple powerups collected

    [SerializeField] Vector3 _startPosition = default;
    [SerializeField] float _speed = 1f;
    [SerializeField] float _speedBoostMuliplier = 2f;
    [SerializeField] string _horizontalAxis = null;
    [SerializeField] string _verticalAxis = null;
    [SerializeField] float _topBound = default;
    [SerializeField] float _bottomBound = default;
    [SerializeField] float _rightBound = default;
    [SerializeField] float _leftBound = default;
    [SerializeField] GameObject _lazerPrefab = default;
    [SerializeField] GameObject _tripleShotPrefab = default;
    [SerializeField] GameObject _lazerContainer = default;
    [SerializeField] Vector3 _lazerPositionOffset = default;
    [SerializeField] float _fireRate = default;
    [SerializeField] int _lives = 3;
    [SerializeField] bool _isTripleShotActive = false;
    [SerializeField] bool _isSpeedBoostActive = false;
    [SerializeField] GameObject _shields = default;
    [SerializeField] bool _areShieldsActive = false;
    [SerializeField] GameObject[] _damageFX = default;
    [SerializeField] UnityEvent _playerDead = default;
    [SerializeField] AudioClip _lazerSFX = default;
    [SerializeField] AudioClip _tripleLazerSFX = default;
    [SerializeField] GameObject _expolsion = default;
    [SerializeField] AudioClip _explosionSFX = default;


    //Variables
    float _canFireTimer = 0;
    UIManger _uIManager;
    int _damageIndex = 0;
    AudioSource _audioSource;
    [SerializeField] List<PowerUp> currentPowerUps = new List<PowerUp>();

    void Start()
    {
        transform.position = _startPosition;
        _shields.SetActive(false);
        _uIManager = FindObjectOfType<UIManger>();
        _uIManager.SetLivesDisplay(_lives);
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _lazerSFX;
    }

    void Update()
    {
        Movement();

        if (Input.GetKey(KeyCode.Space) && Time.time > _canFireTimer)
        {
            Fire();
        }
    }

    private void Movement()
    {
        CalculateMovement();        
        CheckVerticalBounds();
        WrapVerticalPosition();
    }

    /// <summary> Calculates where to move player by getting inputs </summary>
    private void CalculateMovement()
    {
        float horizontalInput = Input.GetAxis(_horizontalAxis);
        float verticalInput = Input.GetAxis(_verticalAxis);
        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);

        transform.Translate(direction * _speed * Time.deltaTime);        
    }

    private void Fire()
    {
        _canFireTimer = Time.time + _fireRate;

        if (_isTripleShotActive)
        {
            Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity, _lazerContainer.transform);
        }
        else
        {
            Vector3 newPosition = transform.position + _lazerPositionOffset;
            Instantiate(_lazerPrefab, newPosition, Quaternion.identity, _lazerContainer.transform);
        }
        _audioSource.Play();

    }

    /// <summary> Sets the vertical boundry of the player movement. Can be changed via the associated variables </summary>
    private void CheckVerticalBounds()
    {
        float yPosition = Mathf.Clamp(transform.position.y, _bottomBound, _topBound);
        transform.position = new Vector3(transform.position.x, yPosition, 0);
    }

    private void WrapVerticalPosition()
    {
        if (transform.position.x >= _rightBound)
        {
            transform.position = new Vector3(_leftBound, transform.position.y, 0);
        }
        else if (transform.position.x <= _leftBound)
        {
            transform.position = new Vector3(_rightBound, transform.position.y, 0);
        }
    }

    public void Damage(int damage) //IDamageable
    {
        if (_lives <= 0) return;
            
        _lives -= damage;

        if (_areShieldsActive)
        {
            _areShieldsActive = false;
            _shields.SetActive(false);
            GetComponent<CircleCollider2D>().enabled = false;
            return;
        }
        ProcessCollision();

        if (_damageIndex <= _damageFX.Length -1)
        {
            _damageFX[_damageIndex].SetActive(true);
            _damageIndex++;
        }   
    }

    public void ProcessCollision() //IDamagable
    {
        _uIManager.SetLivesDisplay(_lives);

        if (_lives <= 0)
        {
            GameObject explosion = Instantiate(_expolsion, transform.position, Quaternion.identity);
            explosion.tag = gameObject.tag;
            AudioSource.PlayClipAtPoint(_explosionSFX, Camera.main.transform.position);
            _playerDead.Invoke();
            gameObject.SetActive(false);
        }
    }

    public void ActivatePowerUp(PowerUpTypes newPowerUp)
    {
        switch (newPowerUp)
        {
            case PowerUpTypes.TripleShot:
                if (_isTripleShotActive == true) return;
                _isTripleShotActive = true;
                _audioSource.clip = _tripleLazerSFX;
                break;
            case PowerUpTypes.SpeedBoost:
                if (_isSpeedBoostActive == true) return;
                _isSpeedBoostActive = true;
                _speed = _speed * _speedBoostMuliplier;
                break;
            case PowerUpTypes.Shield:
                _areShieldsActive = true;
                _shields.SetActive(true);
                GetComponent<CircleCollider2D>().enabled = true;
                break;
            default:
                break;
        }
    }

    public void DeactivatePowerUps(PowerUp oldPowerUp)
    {
        switch (oldPowerUp.ReturnPowerUpType())
        {
            case PowerUpTypes.TripleShot:
                _isTripleShotActive = false;
                _audioSource.clip = _lazerSFX;
                currentPowerUps.Remove(oldPowerUp);
                break;
            case PowerUpTypes.SpeedBoost:
                currentPowerUps.Remove(oldPowerUp);
                _isSpeedBoostActive = false;
                _speed = _speed / _speedBoostMuliplier;
                break;
            default:
                break;
        }
    }

    public void CheckForActivePowerUps(PowerUp newPowerUp)
    {
        foreach (var activePowerUp in currentPowerUps)
        {
            if(activePowerUp.ReturnPowerUpType() == newPowerUp.ReturnPowerUpType())
            {
                currentPowerUps.Remove(activePowerUp);
                currentPowerUps.Add(newPowerUp);
                Destroy(activePowerUp.gameObject);
                return;
            }
        }
        currentPowerUps.Add(newPowerUp);
    }

}
