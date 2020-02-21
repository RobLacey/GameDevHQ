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
    [SerializeField] float _tripleShotTimer = 5f;
    [SerializeField] float _speedBoostTimer = 7f;
    [SerializeField] GameObject _shields = default;
    [SerializeField] bool _areShieldsActive = false;
    [SerializeField] UnityEvent _playerDead = default;

    //Variables
    float _canFireTimer = 0;
    UIManger _uIManager;

    void Start()
    {
        transform.position = _startPosition;
        _shields.SetActive(false);
        _uIManager = FindObjectOfType<UIManger>();
        _uIManager.SetLivesDisplay(_lives);
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

    public void Damage(float damage) //IDamageable
    {
        if (_areShieldsActive)
        {
            _areShieldsActive = false;
            _shields.SetActive(false);
            GetComponent<CircleCollider2D>().enabled = false;
            return;
        }

        _lives--;
        _uIManager.SetLivesDisplay(_lives);

        if (_lives <= 0)
        {
            _playerDead.Invoke();
            Destroy(gameObject);
        }
    }

    public void ActivatePowerUp(PowerUpTypes powerUp)
    {
        switch (powerUp)
        {
            case PowerUpTypes.TripleShot:
                StartCoroutine(TripleShot());
                break;
            case PowerUpTypes.SpeedBoost:
                StartCoroutine(SpeedBoost());
                break;
            case PowerUpTypes.Shield:
                Shields();
                break;
            default:
                Debug.Log("Non found");
                break;
        }
    }

    private IEnumerator TripleShot()
    {
        _isTripleShotActive = true;
        yield return new WaitForSeconds(_tripleShotTimer);
        _isTripleShotActive = false;
    }

    private IEnumerator SpeedBoost()
    {
        _speed = _speed * _speedBoostMuliplier;
        yield return new WaitForSeconds(_speedBoostTimer);
        _speed = _speed / _speedBoostMuliplier;
    }

    private void Shields()
    {
        _areShieldsActive = true;
        _shields.SetActive(true);
        GetComponent<CircleCollider2D>().enabled = true;
    }
}
