using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] TeamID _teamID;
    [SerializeField] float _health = default;
    [SerializeField] int _damageDealt = 1;
    [SerializeField] bool _hasHealthBar;
    [SerializeField] EventManager _Event_WaveWipedCancel = default;

    public TeamID I_TeamTag { get { return _teamID; } }

    //Variables
    float _instanceHealth;
    IKillable[] _killables;
    bool _collisionKill = false;
    Slider _healthBar;
    IShowDamage _showDamage;

    private void Awake()
    {
        _showDamage = GetComponent<IShowDamage>();

        if (_hasHealthBar)
        {
            _healthBar = GetComponentInChildren<Slider>();
        }
        _killables = GetComponents<IKillable>();
    }

    private void OnEnable()
    {
       _instanceHealth = _health;
       _collisionKill = false;
       if(_hasHealthBar) _healthBar.value = _instanceHealth / _health;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.gameObject.GetComponentInParent<IDamageable>();

        if (canDealDamage != null && canDealDamage.I_TeamTag != I_TeamTag)
        {
            _Event_WaveWipedCancel.Invoke(0, false, this);
            _collisionKill = true;
            canDealDamage.I_ProcessCollision(_damageDealt);
            I_ProcessCollision(_instanceHealth);
        }
    }

    public void I_ProcessCollision(float damage)
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
        ShowDamage();
    }

    private void UpdateHealthBar()
    {
        if (_hasHealthBar)
        {
            _healthBar.value = _instanceHealth / _health;
        }
    }

    private void ShowDamage()
    {
        if (_showDamage != null)
        {
            _showDamage.DamageDisplay(_instanceHealth / _health);
        }
    }
}
