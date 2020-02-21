using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
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
            Vector3 spawnPosition = toSpawn.GetComponent<ISpawnable>().SpawnPosition();
            Instantiate(toSpawn, spawnPosition, Quaternion.identity, _spawnedContainer.transform);
            yield return new WaitForSeconds(timer);
        }   
    }

    public void PlayerDead() //UE
    {
        canSpawn = false;
    }
}
