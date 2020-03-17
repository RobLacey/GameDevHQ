using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject[] _spawnArray = default;
    [SerializeField] float _minSpawnTime = 1f;
    [SerializeField] float _maxSpawnTime = 1f;
    [SerializeField] float _startDelay = default;
    [SerializeField] bool _trackActiveObjects = false;
    [SerializeField] bool canSpawn = true;
    [SerializeField] EventManager _Event_StartSpawning;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] EventManager _Event_RemoveEnemyASTarget;
    [SerializeField] EventManager _Event_AddEnemy;
    [SerializeField] EventManager _Event_ReturnActiveEnemies;
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] GlobalVariables _mySpawnLimits;

    public List<GameObject> ActiveTargets { get; private set; }

    private void Awake()
    {
        ActiveTargets = new List<GameObject>();
    }

    private void OnEnable()
    {
        _Event_StartSpawning.AddListener(() => StartSpawning());
        _Event_PlayerDead.AddListener(() => PlayerDead());
        if (_trackActiveObjects)
        {
            _Event_ReturnActiveEnemies.AddListener(() => ActiveTargets);
            _Event_RemoveEnemyASTarget.AddListener((x) => RemoveActiveTargets(x));
            _Event_AddEnemy.AddListener((x) => ActiveTargets.Add((GameObject)x));
        }
    }

    private void StartSpawning()
    {
        ActiveTargets = new List<GameObject>();
        StartCoroutine(StartDelay());
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(_startDelay);
        StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        while (canSpawn)
        {
            float timer = Random.Range(_minSpawnTime, _maxSpawnTime);
            GameObject toSpawn = _spawnArray[Random.Range(0, _spawnArray.Length)];
            Vector3 newPos = SpawnPosition();
            _poolingAgent.InstantiateFromPool(toSpawn, newPos, Quaternion.identity);
            yield return new WaitForSeconds(timer);
        }
    }

    private Vector3 SpawnPosition() 
    {
        float randomX = Random.Range(_mySpawnLimits.RightBounds, _mySpawnLimits.LeftBounds);
        return new Vector3(randomX, _mySpawnLimits.TopBounds);
    }

    private void PlayerDead()
    {
        canSpawn = false;
    }

    private void RemoveActiveTargets(object oldtaget)
    {
        GameObject toRemove = (GameObject)oldtaget;

        if(ActiveTargets.Contains(toRemove))
        {
            ActiveTargets.Remove(toRemove);
        }
    }
}
