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
    [SerializeField] float _powerUpStartDelay = 10f;
    [SerializeField] EventManager _Event_StartSpawning;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] PoolingAgent _enemyPoolingAgent;
    [SerializeField] PoolingAgent _powerUpPoolingAgent;
    [SerializeField] GlobalVariables _myVars;

    private void OnEnable()
    {
        _Event_StartSpawning.AddListener(() => StartSpawning());
        _Event_PlayerDead.AddListener(() => PlayerDead());
    }

    private void StartSpawning()
    {
        StartCoroutine(SpawnObjects(_enemyPrefab, _minEnemySpawnTime, _maxEnemySpawnTime, _enemyPoolingAgent, Vector3.zero));
        StartCoroutine(PowerUpSpawnDelay());
    }

    private IEnumerator PowerUpSpawnDelay()
    {
        yield return new WaitForSeconds(_powerUpStartDelay);
        StartCoroutine(SpawnObjects(_powerUps, _minPowerUpSpawnTime, _maxPowerUpSpawnTime, _powerUpPoolingAgent, Vector3.back));
    }

    private IEnumerator SpawnObjects(GameObject[] spawnArray, float minTime, float maxTime, PoolingAgent agent, Vector3 offset)
    {
        while (canSpawn)
        {
            GameObject toSpawn = spawnArray[Random.Range(0, spawnArray.Length)];
            Vector3 newPos = SpawnPosition();
            agent.InstantiateFromPool(toSpawn, newPos + offset, Quaternion.identity);
            float timer = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(timer);
        }
    }

    private Vector3 SpawnPosition() 
    {
        float randomX = Random.Range(_myVars.RightBounds, _myVars.LeftBounds);
        return new Vector3(randomX, _myVars.TopBounds);
    }

    private void PlayerDead()
    {
        canSpawn = false;
    }
}
