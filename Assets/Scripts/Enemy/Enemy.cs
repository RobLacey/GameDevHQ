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

    SpriteRenderer _mySprite;
    Collider2D _collider2D;
    ISpawnable _myWave;

    private void Awake()
    {
        _myWave = GetComponentInParent<ISpawnable>();
        _mySprite = GetComponentInChildren<SpriteRenderer>();
        _collider2D = GetComponentInChildren<Collider2D>();
    }
    private void OnEnable()
    {
        _mySprite.enabled = true;
        _collider2D.enabled = true;
    }

    public void Dead()
    {
        if (_myWave != null)
        {
            _myWave.LostEnemyFromWave();
        }
        _collider2D.enabled = false;
        _mySprite.enabled = false;
        GameObject newObject = _poolingAgent.InstantiateFromPool(_expolsion, transform.position, Quaternion.identity);
        newObject.GetComponent<IScaleable>().SetScale(_myBody.localScale);
        _Event_AddToScore.Invoke(_points);
        gameObject.SetActive(false);
    }

}
