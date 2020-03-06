using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour, ISpawnable
{
    GameObject[] enemies;
    int numberOfEnemiesInWave = 0;
    int enemiesLeft = 0;

    private void Awake()
    {
        int index = 0;
        numberOfEnemiesInWave = transform.childCount;
        enemiesLeft = numberOfEnemiesInWave;
        enemies = new GameObject[numberOfEnemiesInWave];
        foreach (Transform child in transform)
        {
            enemies[index] = child.gameObject;
            index++;
        }
    }

    private void OnEnable()
    {
        ActivateChildObjects(true);
    }

    public void ActivateChildObjects(bool activate)
    {
        enemiesLeft = numberOfEnemiesInWave;
        foreach (var enemy in enemies)
        {
            enemy.SetActive(activate);
        }
    }

    public void LostEnemyFromWave()
    {
        enemiesLeft--;
        if (enemiesLeft <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
