using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IKillable, IWaveMemeber
{
    [SerializeField] int _points = 0;
    [SerializeField] GameObject _expolsion = default;
    [SerializeField] float _expolsionSize = 0.5f;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] GameObject _shrapnel;
    [SerializeField] EventManager _Event_AddToScore = default;
    [SerializeField] EventManager _Event_RemoveEnemyAsTarget;
    [SerializeField] EventManager _Event_AddEnemy;

    SpriteRenderer _mySprite;
    Collider2D _collider2D;
    IEnemyWave _partOfWave;

    public GameObject Myself { get { return gameObject; } }

    private void Awake()
    {
        _partOfWave = GetComponentInParent<IEnemyWave>(); 
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
        if (_partOfWave != null && !collsionKill)
        {
            _partOfWave.I_LostEnemyFromWave(_points, transform.position);
        }
        else
        {
            _Event_AddToScore.Invoke(_points);
        }
        _collider2D.enabled = false;
        _mySprite.enabled = false;
        CreateDeathFX();
        _Event_RemoveEnemyAsTarget.Invoke(gameObject);
        gameObject.SetActive(false);
    }

    private void CreateDeathFX()
    {
        if (_shrapnel != null)
        {
            _poolingAgent.InstantiateFromPool(_shrapnel, transform.position, Quaternion.identity);
        }

        GameObject newObject = _poolingAgent.InstantiateFromPool(_expolsion, transform.position, Quaternion.identity);
        newObject.GetComponent<IScaleable>().I_SetScale(new Vector3(_expolsionSize, _expolsionSize, _expolsionSize));
    }
}
