using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject _enemyPrefab = default;
    [SerializeField] GameObject _spawnedContainer = default;
    [SerializeField] GameObject _powerUp = default;
    [SerializeField] float _minEnemySpawnTime = 1f;
    [SerializeField] float _maxEnemySpawnTime = 1f;
    [SerializeField] float _minPowerUpSpawnTime = 1f;
    [SerializeField] float _maxPowerUpSpawnTime = 1f;
    [SerializeField] bool canSpawn = true;

    private void OnEnable()
    {
        Player._Dead += PlayerDead;
    }

    private void OnDisable()
    {
        Player._Dead -= PlayerDead;
    }

    void Start()
    {
        SpawnRoutine();
    }

    private void SpawnRoutine()
    {
        StartCoroutine(InstantiateNewObject(_enemyPrefab, Random.Range(_minEnemySpawnTime, _maxEnemySpawnTime)));
        StartCoroutine(InstantiateNewObject(_powerUp, Random.Range(_minPowerUpSpawnTime, _maxPowerUpSpawnTime)));
    }

    private IEnumerator InstantiateNewObject(GameObject toSpawn, float timer)
    {
        while (canSpawn)
        {
            Vector3 spawnPosition = toSpawn.GetComponent<NonPlayerMovement>().Spawn();
            Instantiate(toSpawn, spawnPosition, Quaternion.identity, _spawnedContainer.transform);
            yield return new WaitForSeconds(timer);
        }   
    }

    private void PlayerDead()
    {
        canSpawn = false;
    }
}
