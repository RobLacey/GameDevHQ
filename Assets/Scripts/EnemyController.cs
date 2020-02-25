using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] float _speed = default;
    [SerializeField] protected int _health = default;
    [SerializeField] bool _respawn = false;
    [SerializeField] bool _usesForce = false;
    [SerializeField] int _damageDealt = 1;
    [SerializeField] int _points = default;
    [SerializeField] GameObject _expolsion = default;


    SpawnManager _spawnManager;
    protected SpriteRenderer _myBody;
    UIManger _uiManger;
    protected AudioSource _audioSource;

    protected virtual void Awake()
    {
        _spawnManager = FindObjectOfType<SpawnManager>();
        _myBody = GetComponentInChildren<SpriteRenderer>();
        _uiManger = FindObjectOfType<UIManger>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!_usesForce)
        {
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
        }
        OffScreenCheck();
    }

    private void OffScreenCheck()
    {
        if (transform.position.y < _spawnManager.ReturnBottomBounds())
        {
            if (_respawn)
            {
                transform.position = _spawnManager.SpawnPosition();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Player canDealDamage = collision.gameObject.GetComponent<Player>();

        if (canDealDamage != null)
        {
            canDealDamage.GetComponent<IDamageable>().Damage(_damageDealt);
            Damage(_health);
        }
    }

    public void SetSpeed(float randomSpeed)
    {
        _speed = randomSpeed;
    }

    public void Damage(int damage)
    {
        _health -= damage;
        ProcessCollision();
    }

    public virtual void ProcessCollision()
    {
        _audioSource.Play();
        _uiManger.AddToScore(_points);
        GetComponent<Collider2D>().enabled = false;
        _myBody.enabled = false;
        Instantiate(_expolsion, transform.position, Quaternion.identity, transform);
    }
}
