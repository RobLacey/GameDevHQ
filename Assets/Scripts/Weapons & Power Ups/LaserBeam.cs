using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour, ITagable
{
    [SerializeField] float speed = 10f;
    [SerializeField] int damage = 1;
    [SerializeField] string _myTeamTag = default;
    [SerializeField] bool _savePositionAtStart = default;
    [SerializeField] GameObject _hiteffect;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] GlobalVariables _myVars;
    [SerializeField] Vector3 _direction;

    float _bounds;
    Vector3[] _startingPos;

    public int TeamTag { get; set; }

    private void Awake()
    {
        TeamTag = _myTeamTag.GetHashCode();
        if (_savePositionAtStart)
        {
            StorePosition();
        }
    }

    private void OnEnable()
    {
        if (_savePositionAtStart)
        {
            RestorePosition();
        }
    }

    private void StorePosition()
    {
        int index = 0;
        foreach (Transform item in transform)
        {
             _startingPos[index] = item.transform.position;
            index++;
        }
    }

    private void RestorePosition()
    {
        int index = 0;
        foreach (Transform item in transform)
        {
             item.transform.position = _startingPos[index];
            index--;
        }
    }

    void Update()
    {
        transform.Translate(_direction * speed * Time.deltaTime);
        OffScreenCheck();
    }

    private void OffScreenCheck() 
    {
        if (transform.position.y > _myVars.TopBounds || transform.position.y < _myVars.BottomBounds)
        {
            gameObject.SetActive(false);
        }

        if (transform.position.x < _myVars.LeftBounds || transform.position.x >_myVars.RightBounds)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.GetComponentInParent<IDamageable>();
        bool isAPowerUp = collision.GetComponent<PowerUp>();

        if (canDealDamage != null && !isAPowerUp)
        {
            if (canDealDamage.TeamTag != TeamTag)
            {
                canDealDamage.ProcessCollision(damage);
                _poolingAgent.InstantiateFromPool(_hiteffect, transform.position, Quaternion.identity);
                gameObject.SetActive(false);
            }
        }
    }
}
