using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject[] _enemyPrefab = default;
    [SerializeField] GameObject _spawnedContainer = default;
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

    public void StartSpawning()
    {
        StartCoroutine(SpawnObjects(_enemyPrefab, _minEnemySpawnTime, _maxEnemySpawnTime));
        StartCoroutine(PowerUpSpawnDelay());
    }

    private IEnumerator PowerUpSpawnDelay()
    {
        yield return new WaitForSeconds(_startDelayTimer);
        StartCoroutine(SpawnObjects(_powerUps, _minPowerUpSpawnTime, _maxPowerUpSpawnTime));
    }

    private IEnumerator SpawnObjects(GameObject[] spawnArray, float minTime, float maxTime)
    {
        while (canSpawn)
        {
            GameObject toSpawn = spawnArray[Random.Range(0, spawnArray.Length)];
            float timer = Random.Range(minTime, maxTime);
            Vector3 spawnPosition = SpawnPosition();
            Instantiate(toSpawn, spawnPosition, Quaternion.identity, _spawnedContainer.transform);
            yield return new WaitForSeconds(timer);
        }
    }

    public Vector3 SpawnPosition() 
    {
        float randomX = Random.Range(_rightBounds, _LeftBounds);
        return new Vector3(randomX, _topBounds);
    }

    public float ReturnBottomBounds()
    {
        return _bottomBounds;
    }


    public void PlayerDead() //UE
    {
        canSpawn = false;
    }
}
