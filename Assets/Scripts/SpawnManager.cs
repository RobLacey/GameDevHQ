using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject[] _enemyPrefab = default;
    [SerializeField] GameObject[] _powerUps = default;
    [SerializeField] float _minEnemySpawnTime = 1f;
    [SerializeField] float _maxEnemySpawnTime = 1f;
    [SerializeField] float _minPowerUpSpawnTime = 1f;
    [SerializeField] float _maxPowerUpSpawnTime = 1f;
    [SerializeField] bool canSpawn = true;
    [SerializeField] float _startDelayTimer = 10f;
    [SerializeField] float _topBounds = default;
    [SerializeField] float _bottomBounds = default;
    [SerializeField] float _rightBounds = default;
    [SerializeField] float _LeftBounds = default;
    [SerializeField] EventManager _Event_Return_BottomBounds;
    [SerializeField] EventManager _Event_ReturnTopBounds = default;
    [SerializeField] EventManager _Event_StartSpawning;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] PoolingAgent _enemyPoolingAgent;
    [SerializeField] PoolingAgent _powerUpPoolingAgent;

    //Properties
    public float BottomBounds { get { return _bottomBounds; } }
    public float TopBounds { get { return _topBounds; } }

    //Variables

    private void OnEnable()
    {
        _Event_ReturnTopBounds.AddListener(() => TopBounds);
        _Event_Return_BottomBounds.AddListener(() => BottomBounds);
        _Event_StartSpawning.AddListener(() => StartSpawning());
        _Event_PlayerDead.AddListener(() => PlayerDead());
    }

    public void StartSpawning()
    {
        StartCoroutine(SpawnObjects(_enemyPrefab, _minEnemySpawnTime, _maxEnemySpawnTime, _enemyPoolingAgent, Vector3.zero));
        StartCoroutine(PowerUpSpawnDelay());
    }

    private IEnumerator PowerUpSpawnDelay()
    {
        yield return new WaitForSeconds(_startDelayTimer);
        StartCoroutine(SpawnObjects(_powerUps, _minPowerUpSpawnTime, _maxPowerUpSpawnTime, _powerUpPoolingAgent, Vector3.back));
    }

    private IEnumerator SpawnObjects(GameObject[] spawnArray, float minTime, float maxTime, PoolingAgent agent, Vector3 offset)
    {
        while (canSpawn)
        {
            GameObject toSpawn = spawnArray[Random.Range(0, spawnArray.Length)];
            Vector3 newPos = SpawnPosition();
            agent.InstantiateFromPool(toSpawn, newPos + offset);
            float timer = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(timer);
        }
    }

    public Vector3 SpawnPosition() 
    {
        float randomX = Random.Range(_rightBounds, _LeftBounds);
        return new Vector3(randomX, _topBounds);
    }

    private void PlayerDead()
    {
        canSpawn = false;
    }
}
