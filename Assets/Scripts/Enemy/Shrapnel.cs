using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrapnel : MonoBehaviour
{
    [SerializeField] float _minSpeed = 1f;
    [SerializeField] float _maxSpeed = 5f;
    [SerializeField] int _damage = 2;
    [SerializeField] TeamID _teamTag;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] GameObject _damageFX;
    [SerializeField] GlobalVariables _screenBounds;

    float _speed;
    IShrapnel _shrapnelController;

    private void Awake()
    {
        _shrapnelController = GetComponentInParent<IShrapnel>();
    }

    private void OnEnable()
    {
        _speed = Random.Range(_minSpeed, _maxSpeed);
        transform.SetRotationZ(Random.Range(0, 360f));
    }

    private void Update()
    {
        transform.Translate(transform.up * _speed * Time.deltaTime);
        OffScreenCheck();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDamage = collision.GetComponentInParent<IDamageable>();

        if (canDamage != null && _teamTag != canDamage.I_TeamTag)
        {
            canDamage.I_ProcessCollision(_damage);
            _shrapnelController.I_DeactivateChildObjects();
            _poolingAgent.InstantiateFromPool(_damageFX, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }

    private void OffScreenCheck()
    {
        if (!transform.StillOnScreen(_screenBounds))
        {
            gameObject.SetActive(false);
            _shrapnelController.I_DeactivateChildObjects();
        }
    }
}
