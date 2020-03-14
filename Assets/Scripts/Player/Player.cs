﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour, IDamageable, ISpeedBoostable
{
    //TODO Check and Tidy code
    //TODO Make Power Up colours match UI and add text to capsules
    //TODO Check garbage collection
    //TODO Make sonic bomb
    //TODO add level progression and boss battle
    //TODO add high score table - playerperf
    //TODO First start Event to stop unneccessary Onenable calls and allow Start to get called as stoppped from pooling manager
    

    [SerializeField] TeamID _teamID;
    [SerializeField] Vector3 _startPosition = default;
    [SerializeField] float _speed = 1f;
    [SerializeField] string _horizontalAxis = null;
    [SerializeField] string _verticalAxis = null;
    [SerializeField] float _health = 10;
    [SerializeField] GameObject[] _damageFX = default;
    [SerializeField] GameObject _deathFX = default;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] bool cheat = false;
    [SerializeField] EventManager _Event_PlayerDead = default;
    [SerializeField] EventManager _Event_SetLives = default;
    [SerializeField] EventManager _Event_AddHealth;
    [SerializeField] EventManager _Event_WaveWipedCancel = default;
    [SerializeField] EventManager _Event_DeactivatePowerUp;

    [SerializeField] GlobalVariables _myVars;


    //Properties
    public float I_SetSpeed { set { _speed = value; } }
    public TeamID I_TeamTag { get { return _teamID; } }


    //Variables
    float _canFireTimer = 0;
    float _startingHealth = 0;
    int _damageIndex = 0;
    IWeaponSystem _myWeaponSystem;
    IPowerUpSystem _myPowerUpSystem;

    private void Awake()
    {
        _myWeaponSystem = GetComponent<IWeaponSystem>();
        _myPowerUpSystem = GetComponent<IPowerUpSystem>();
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
            _canFireTimer = Time.time + _myWeaponSystem.I_ReturnFireRate();
            _myWeaponSystem.I_Fire();
        }

    }

    private void Movement()
    {
        CalculateMovement();        
        CheckBounds();
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
    private void CheckBounds()
    {
        float yPosition = Mathf.Clamp(transform.position.y, _myVars.BottomBounds, _myVars.TopBounds);
        float xPosition = Mathf.Clamp(transform.position.x, _myVars.LeftBounds, _myVars.RightBounds);
        transform.position = new Vector3(xPosition, yPosition, 0);
    }

    private void Damage(int damage)
    {
        if (!cheat)
        {
            if (_health <= 0) return;

            if (_myPowerUpSystem.I_ShieldsAreActive)
            {
                _Event_DeactivatePowerUp.Invoke(PowerUpTypes.Shield);
                return;
            }
            _Event_WaveWipedCancel.Invoke(0, false);
            RemoveHealth(damage);
        }
    }

    private void RemoveHealth(int damage)
    {
        _health -= damage;
        float _newHealth = _health / _startingHealth;
        _Event_SetLives.Invoke(_newHealth);
        HealthDisplay(true);
    }

    private void AddHealth()
    {
        if (_health == _startingHealth) return;
        _health++;
        float _newHealth = _health / _startingHealth;
        _Event_SetLives.Invoke(_newHealth);
        _damageIndex--;
        HealthDisplay(false);
    }

    private void HealthDisplay(bool active)
    {
        if (_damageIndex <= _damageFX.Length - 1)
        {
            _damageFX[_damageIndex].SetActive(active);
        }            
        _damageIndex++;
    }


    public void I_ProcessCollision(int damage)
    {
        Damage(damage);

        if (_health <= 0)
        {
            _poolingAgent.InstantiateFromPool(_deathFX, transform.position, Quaternion.identity);
            _Event_PlayerDead.Invoke();
            gameObject.SetActive(false);
        }
    }
}
