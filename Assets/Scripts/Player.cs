using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour, IDamageable
{
    //TODO add rapid fire to speed boost that works with all weapons
    //TODO make enemy ring buffer and change respawn at bottom (keep for asteriods)
    //TODO sideways movement to enemies
    //TODO add health pickup
    //TODO Breakout damage into health class
    //TODO have pickup that is capsule you must shoot first
    //TODO make homing missles
    //TODO Split off weapons and powerups in different classes??
    //TODO add some other enemy ship types
    //TODO Check and Tidy code
    //TODO maybe make it 3d/2.5D and find 3d ships and asteriods etc??
    //TODO add minion helper weapon. Rotates around player and shoots at same time plus acts like a temp shield if hit it is destroyed.
    //TODO Check garbage collection

    [SerializeField] Vector3 _startPosition = default;
    [SerializeField] float _speed = 1f;
    [SerializeField] string _horizontalAxis = null;
    [SerializeField] string _verticalAxis = null;
    [SerializeField] float _topBound = default;
    [SerializeField] float _bottomBound = default;
    [SerializeField] float _rightBound = default;
    [SerializeField] float _leftBound = default;
    [SerializeField] int _lives = 3;
    [SerializeField] GameObject[] _damageFX = default;
    [SerializeField] GameObject _deathFX = default;
    [SerializeField] UnityEvent _playerDead = default;


    //Variables
    float _canFireTimer = 0;
    UIManger _uIManager;
    int _damageIndex = 0;
    WeaponsSystem _myWeaponSystem;

    private void Awake()
    {
        _uIManager = FindObjectOfType<UIManger>();
        _myWeaponSystem = GetComponent<WeaponsSystem>();
    }

    void Start()
    {
        transform.position = _startPosition;
        _uIManager.SetLivesDisplay(_lives);
    }

    void Update()
    {
        Movement();
        Fire();
    }

    private void Fire()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time > _canFireTimer)
        {
            _canFireTimer = Time.time + _myWeaponSystem.ReturnFireRate();
            _myWeaponSystem.Fire();
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
            
        if (_myWeaponSystem.ReturnShieldActive())
        {
            _myWeaponSystem.DeactivatePowerUps(PowerUpTypes.Shield);
            return;
        }

        _lives -= damage;
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
            GameObject explosion = Instantiate(_deathFX, transform.position, Quaternion.identity);
            explosion.tag = gameObject.tag;
            _playerDead.Invoke();
            gameObject.SetActive(false);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }
}
