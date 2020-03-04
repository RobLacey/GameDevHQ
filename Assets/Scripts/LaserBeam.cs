using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class LaserBeam : MonoBehaviour, ITagable
{
    [SerializeField] float speed = 10f;
    [SerializeField] int damage = 1;
    [SerializeField] string _myTag = default;
    [SerializeField] bool _isEnemyWeapon = default;
    [SerializeField] EventManager _Event_ReturnBounds = default;
    [SerializeField] float _boundsAdjustment = default;
    [SerializeField] GameObject _hiteffect;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] Vector3 _damageFXOffset;

    float _bounds;

    public string SetTagName { set { _myTag = value; } }

    private void Start()
    {
        _bounds = (float)_Event_ReturnBounds.Return_Parameter() - _boundsAdjustment;
    }

    void Update()
    {
        if (_isEnemyWeapon)
        {
            transform.Translate(-transform.up * speed * Time.deltaTime);
            OffBottomScreenCheck();
        }
        else
        {
            transform.Translate(transform.up * speed * Time.deltaTime);
            OffTopScreenCheck();
        }
    }

    private void OffTopScreenCheck()
    {
        if (transform.position.y > _bounds)
        {
            gameObject.SetActive(false);
        }
    }

    private void OffBottomScreenCheck()
    {
        if (transform.position.y < _bounds)
        {
            gameObject.SetActive(false);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable []canDealDamage = collision.GetComponents<IDamageable>();
        bool isAPowerUp = collision.GetComponent<PowerUp>();

        if (canDealDamage != null && !isAPowerUp)
        {
            if (collision.gameObject.tag != _myTag)
            {
                foreach (var item in canDealDamage)
                {
                    item.Damage(damage);
                }
                _poolingAgent.InstantiateFromPool(_hiteffect, collision.transform.position + _damageFXOffset);
                gameObject.SetActive(false);
            }
        }
    }
}
