using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveController : MonoBehaviour, IEnemyWave
{
    [SerializeField] PoolingAgent _poolingAgent;
    [SerializeField] GameObject _wipeUI;
    [SerializeField] EventManager _Event_WaveWiped = default;
    [SerializeField] EventManager _Event_AddToScore = default;

    //Variables
    GameObject[] enemies;
    int numberOfEnemiesInWave;
    int enemiesLeft = 0;
    Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
        int index = 0;
        numberOfEnemiesInWave = transform.childCount;
        enemiesLeft = numberOfEnemiesInWave;
        enemies = new GameObject[enemiesLeft];
        foreach (Transform child in transform)
        {
            enemies[index] = child.gameObject;
            index++;
        }
    }

    private void OnEnable()
    {
        ActivateChildObjects();
    }

    private void ActivateChildObjects()
    {
        enemiesLeft = numberOfEnemiesInWave;
        foreach (var enemy in enemies)
        {
            enemy.SetActive(true);
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

    public void I_LostEnemyFromWave(int score, Vector3 lastEnemiesPosition)
    {
        enemiesLeft--;
        if (enemiesLeft <= 0)
        {
            _Event_WaveWiped.Invoke(score, true);
            gameObject.SetActive(false);
            GameObject newObject = _poolingAgent.InstantiateFromPool(_wipeUI, transform.position, Quaternion.identity);
            newObject.GetComponentInChildren<Text>().transform.position = GetScreenPosition(lastEnemiesPosition);
        }
        else
        {
            _Event_AddToScore.Invoke(score);
        }
    }

    private Vector3 GetScreenPosition(Vector3 position)
    {
        return _mainCamera.WorldToScreenPoint(position);
    }
}
