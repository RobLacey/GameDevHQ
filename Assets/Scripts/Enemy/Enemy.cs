using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IKillable
{
    [SerializeField] int _points = 0;
    [SerializeField] GameObject _expolsion = default;
    [SerializeField] Transform _myBody;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] EventManager _Event_AddToScore = default;
    [SerializeField] EventManager _Event_RemoveEnemyAsTarget;
    [SerializeField] EventManager _Event_AddEnemy;


    SpriteRenderer _mySprite;
    Collider2D _collider2D;
    IEnemyWave _myWave;

    private void Awake()
    {
        _myWave = GetComponentInParent<IEnemyWave>(); //TODO maybe change to event
        _mySprite = GetComponentInChildren<SpriteRenderer>();
        _collider2D = GetComponentInChildren<Collider2D>();
    }
    private void OnEnable()
    {
        _mySprite.enabled = true;
        _collider2D.enabled = true;
        _Event_AddEnemy.Invoke(gameObject);
    }

    public void I_Dead(bool collsionKill)
    {
        if (_myWave != null && !collsionKill)
        {
            _myWave.I_LostEnemyFromWave(_points);
        }
        else
        {
            _Event_AddToScore.Invoke(_points);
        }
        _collider2D.enabled = false;
        _mySprite.enabled = false;
        GameObject newObject = _poolingAgent.InstantiateFromPool(_expolsion, transform.position, Quaternion.identity);
        newObject.GetComponent<IScaleable>().I_SetScale(_myBody.localScale);
        _Event_RemoveEnemyAsTarget.Invoke(gameObject);
        gameObject.SetActive(false);
    }

}
