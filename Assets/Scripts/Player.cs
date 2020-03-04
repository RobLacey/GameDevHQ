using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour, IDamageable, ISpeedBoostable
{
    //TODO add sfx to enemy lazers, player hits, asteriod hits
    //TODO  add hit fx when player or asteriod hit
    //TODO add rapid fire to speed boost that works with all weapons
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
    [SerializeField] int _health = 3;
    [SerializeField] GameObject[] _damageFX = default;
    [SerializeField] GameObject _deathFX = default;
    [SerializeField] EventManager _Event_PlayerDead = default;
    [SerializeField] EventManager _Event_SetLives = default;
    [SerializeField] EventManager _Event_AddHealth;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] bool cheat = false;

    //Properties
    public float SetSpeed { set { _speed = value; } }

    //Variables
    float _canFireTimer = 0;
    int _startingHealth = 0;
    int _damageIndex = 0;
    IWeaponSystem _myWeaponSystem;

    private void Awake()
    {
        _myWeaponSystem = GetComponent<IWeaponSystem>();
        if (cheat)
        {
            Debug.Log("Cheat ON");
        }
    }

    private void OnEnable()
    {
        _Event_AddHealth.AddListener(() => AddHealth());
    }

    void Start()
    {
        transform.position = _startPosition;
        _startingHealth = _health;
        _Event_SetLives.Invoke(_health);
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

    public void Damage(int damage)
    {
        if (!cheat)
        {
            if (_health <= 0) return;

            if (_myWeaponSystem.ShieldsAreActive)
            {
                _myWeaponSystem.DeactivatePowerUps(PowerUpTypes.Shield);
                return;
            }

            _health -= damage;
            ProcessCollision();
            HealthDisplay(true);
            _damageIndex++;
        }
    }

    private void HealthDisplay(bool active)
    {
        if (_damageIndex <= _damageFX.Length - 1)
        {
            _damageFX[_damageIndex].SetActive(active);
        }
    }

    private void AddHealth()
    {
        if (_health == _startingHealth) return;
        _health++;
        _damageIndex--;
        HealthDisplay(false);
        _Event_SetLives.Invoke(_health);
    }

    public void ProcessCollision()
    {
        _Event_SetLives.Invoke(_health);

        if (_health <= 0)
        {
            _poolingAgent.InstantiateFromPool(_deathFX, transform.position);
            _Event_PlayerDead.Invoke();
            gameObject.SetActive(false);
        }
    }
}
