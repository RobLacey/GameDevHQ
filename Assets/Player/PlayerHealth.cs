using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    //TODO Make more eneries and enemy weapons
    //TODO Speed boost makes engine flame larger
    //TODO Pickup - Time extender, choose to activate and doubles usage time on pickup
    //TODO Warp Bomb
    //TODO Add Music fades
    //TODO Let enemy past loose points
    //TODO large ships/larger squadrons worth bigger muiltpier
    //TODO make asteriods easier to kill
    //TODO Adjust HighScore input as number too short
    //TODO add in exclude from build sections
    //TODO Check and Tidy code
    //TODO Check garbage collection
    //TODO add level progression and boss battle
    //TODO Start Cinematic

    [SerializeField] TeamID _teamID;
    [SerializeField] float _health = 10;
    [SerializeField] GameObject _damageFX = default;
    [SerializeField] AudioClip _damageSoundFX = default;
    [SerializeField] float _sfxVolume = 1f;
    [SerializeField] GameObject _deathFX = default;
    [SerializeField] Vector3 _explosionSize = default;
    [SerializeField] bool cheat = false;
    [SerializeField] EventManager _Event_PlayerDead = default;
    [SerializeField] EventManager _Event_SetLives = default;
    [SerializeField] EventManager _Event_AddHealth;
    [SerializeField] EventManager _Event_WaveWipedCancel = default;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_Are_Shields_Active;

    //Properties
    public TeamID I_TeamTag { get { return _teamID; } }

    //Variables
    float _startingHealth = 0;
    IShowDamage _myDamage;
    AudioSource _myAudioSource;
    PoolingAgent _poolingAgent;

    private void Awake()
    {
        _startingHealth = _health;
        _poolingAgent = _poolingAgent.SetUpPoolingAgent(GetComponents<PoolingAgent>(), PoolingID.FX);
        _myDamage = GetComponent<IShowDamage>();
        _myAudioSource = GetComponent<AudioSource>();
        if (cheat)
        {
            Debug.Log("Cheat ON");
        }
    }

    private void OnEnable()
    {
        _Event_AddHealth.AddListener(() => AddHealth(), this);
    }

    private void Start()
    {
        _Event_SetLives.Invoke(_health / _startingHealth, this);
    }

    private void Damage(float damage)
    {
        if (!cheat)
        {
            if (_health <= 0) return;

            if ((bool)_Event_Are_Shields_Active.ReturnParameter(this))
            {
                _Event_DeactivatePowerUp.Invoke(PowerUpTypes.Shield, this);
                return;
            }
            _Event_WaveWipedCancel.Invoke(0, false, this);
            _damageFX.SetActive(true);
            _myAudioSource.PlayOneShot(_damageSoundFX, _sfxVolume);
            RemoveHealth(damage);
        }
    }

    private void RemoveHealth(float damage)
    {
        _health -= damage;
        float newHealth = _health / _startingHealth;
        _Event_SetLives.Invoke(newHealth, this);
        _myDamage.DamageDisplay(newHealth);
    }

    private void AddHealth()
    {
        if (_health == _startingHealth) return;
        _health++;
        float newHealth = _health / _startingHealth;
        _Event_SetLives.Invoke(newHealth, this);
        _myDamage.DamageDisplay(newHealth);
    }

    public void I_ProcessCollision(float damage)
    {
        Damage(damage);

        if (_health <= 0)
        {
            GameObject explosion = _poolingAgent.InstantiateFromPool(_deathFX, transform.position, Quaternion.identity);
            explosion.GetComponent<IScaleable>().I_SetScale(_explosionSize);
            _Event_PlayerDead.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
