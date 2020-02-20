using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject _enemyPrefab = default;
    [SerializeField] GameObject _enemyContainer = default;
    [SerializeField] float _waitTime = 5f;
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
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (canSpawn)
        {
            Vector3 spawnPosition = _enemyPrefab.GetComponent<Enemy>().Spawn();
            Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity, _enemyContainer.transform);
            yield return new WaitForSeconds(_waitTime);
        }    
    }

    private void PlayerDead()
    {
        canSpawn = false;
    }
}
