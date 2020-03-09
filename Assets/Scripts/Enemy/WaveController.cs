using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour, IEnemyWave
{
    GameObject[] enemies;
    int numberOfEnemiesInWave = 0;
    int enemiesLeft = 0;
    [SerializeField] EventManager _Event_WaveWiped = default;
    [SerializeField] EventManager _Event_AddToScore = default;


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
        I_ActivateChildObjects(true);
    }

    public void I_ActivateChildObjects(bool activate)
    {
        enemiesLeft = numberOfEnemiesInWave;
        foreach (var enemy in enemies)
        {
            enemy.SetActive(activate);
        }
    }

    public void I_DeactivateChildObjects()
    {
        if (enemiesLeft <= 0)
        {
            foreach (var enemy in enemies)
            {
                enemy.SetActive(false);
            }
        }
        gameObject.SetActive(false);
    }

    public void I_LostEnemyFromWave(int score)
    {
        enemiesLeft--;
        if (enemiesLeft <= 0)
        {
            _Event_WaveWiped.Invoke(score, true);
            gameObject.SetActive(false);
        }
        else
        {
            _Event_AddToScore.Invoke(score);
        }
    }
}
