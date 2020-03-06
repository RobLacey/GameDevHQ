using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] int _health = default;
    [SerializeField] int _damageDealt = 1;
    [SerializeField] string _teamTag = default;

    public int TeamTag { get; set; }

    //Variables
    int _instanceHealth;
    IKillable[] _killables;

    private void Awake()
    {
        TeamTag = _teamTag.GetHashCode();
        _killables = GetComponents<IKillable>();
    }

    private void OnEnable()
    {
       _instanceHealth = _health;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.gameObject.GetComponentInParent<IDamageable>();

        if (canDealDamage != null && canDealDamage.TeamTag != TeamTag)
        {
            canDealDamage.ProcessCollision(_damageDealt);
            ProcessCollision(_instanceHealth);
        }
    }

    public void ProcessCollision(int damage)
    {
        _instanceHealth -= damage;

        if (_instanceHealth <= 0)
        {
            foreach (var item in _killables)
            {
                item.Dead();
            }
        }
    }
}
