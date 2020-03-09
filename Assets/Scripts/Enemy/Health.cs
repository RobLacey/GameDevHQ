using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] TeamID _teamID;
    [SerializeField] int _health = default;
    [SerializeField] int _damageDealt = 1;
    [SerializeField] EventManager _Event_WaveWipedCancel = default;
    [SerializeField] bool _hasHealthBar;


    public TeamID I_TeamTag { get { return _teamID; } }

    //Variables
    int _instanceHealth;
    IKillable[] _killables;
    bool _collisionKill = false;
    Slider _healthBar;

    private void Awake()
    {
        Debug.Log("Enemies not recycling properly so missles get confused");
        if (_hasHealthBar)
        {
            _healthBar = GetComponentInChildren<Slider>();
            _healthBar.maxValue = _health;
        }
        _killables = GetComponents<IKillable>();
    }

    private void OnEnable()
    {
       _instanceHealth = _health;
       _collisionKill = false;
       if(_hasHealthBar) _healthBar.value = _health;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.gameObject.GetComponentInParent<IDamageable>();

        if (canDealDamage != null && canDealDamage.I_TeamTag != I_TeamTag)
        {
        Debug.Log(collision.gameObject.name);
            _Event_WaveWipedCancel.Invoke(0, false);
            _collisionKill = true;
            canDealDamage.I_ProcessCollision(_damageDealt);
            I_ProcessCollision(_instanceHealth);
        }
    }

    public void I_ProcessCollision(int damage)
    {
        _instanceHealth -= damage;

        if (_instanceHealth <= 0)
        {
            foreach (var item in _killables)
            {
                item.I_Dead(_collisionKill);
            }
        }
        UpdateHealthBar(); 
    }

    private void UpdateHealthBar()
    {
        if (_hasHealthBar)
        {
            _healthBar.value = _instanceHealth;
        }
    }
}
