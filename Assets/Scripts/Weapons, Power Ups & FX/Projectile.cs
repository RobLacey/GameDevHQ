using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] TeamID _teamID;
    [SerializeField] int damage = 1;
    [SerializeField] GameObject _hiteffect;
    [SerializeField] GlobalVariables _weaponHitBounds;

    //Variables
    Vector3 _startingPos;
    PoolingAgent _poolingAgent;

    //Properties
    public TeamID I_TeamTag { get { return _teamID; } }

    private void Awake()
    {
        _poolingAgent = _poolingAgent.SetUpPoolingAgent(GetComponents<PoolingAgent>(), PoolingID.FX);
        _startingPos = transform.localPosition;
    }

    protected virtual void OnEnable()
    {
        transform.localPosition = _startingPos;
    }

    protected virtual void Update()
    {
        if (!transform.StillOnScreen(_weaponHitBounds))
        {
            CheckOtherProjectiles();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.GetComponentInParent<IDamageable>();
        bool isAPowerUp = collision.GetComponent<PowerUp>();

        if (canDealDamage != null && !isAPowerUp)
        {
            if (canDealDamage.I_TeamTag != I_TeamTag)
            {
                canDealDamage.I_ProcessCollision(damage);
                _poolingAgent.InstantiateFromPool(_hiteffect, transform.position, Quaternion.identity);
                CheckOtherProjectiles();
            }
        }
    }

    private void CheckOtherProjectiles()
    {
        gameObject.SetActive(false);

        foreach (Transform item in transform.parent)
        {
            if (item.gameObject.activeSelf)
            {
                return;
            }
        }
        transform.parent.gameObject.SetActive(false);
    }

}
