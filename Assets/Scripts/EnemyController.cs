using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] int _health = default;
    [SerializeField] int _damageDealt = 1;
    [SerializeField] int _points = 0;
    [SerializeField] GameObject _expolsion = default;
    [SerializeField] EventManager _Event_AddToScore = default;
    [SerializeField] protected string _playerTag = default;
    [SerializeField] PoolingAgent _poolingAgent;

    //Variables
    protected SpriteRenderer _myBody;
    protected AudioSource _audioSource;
    protected Collider2D _collider2D;
    protected int _instanceHealth;

    protected virtual void OnEnable()
    {
       _myBody.enabled = true;
       _collider2D.enabled = true;
       _instanceHealth = _health;
    }

    protected virtual void Awake()
    {
        _myBody = GetComponentInChildren<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _collider2D = GetComponent<Collider2D>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.gameObject.GetComponent<IDamageable>();

        if (canDealDamage != null && collision.tag == _playerTag)
        {
            canDealDamage.Damage(_damageDealt);
            Damage(_instanceHealth);
        }
    }

    public void Damage(int damage)
    {
        _instanceHealth -= damage;
        ProcessCollision();
    }

    public virtual void ProcessCollision()
    {
        _Event_AddToScore.Invoke(_points);
        _collider2D.enabled = false;
        _myBody.enabled = false;
        GameObject newObject = _poolingAgent.InstantiateFromPool(_expolsion, transform.position);
        newObject.GetComponent<IScaleable>().SetScale(transform.localScale);
    }
}
