using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] EventManager _Event_StartLevel;

    bool _isGameOver { get; set; }

    private void OnEnable()
    {
        _Event_PlayerDead.AddListener(() => GameOver());
    }

    private void Start()
    {
        _Event_StartLevel.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Quit");
        }

        if (Input.GetKeyDown(KeyCode.R) && _isGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.M) && _isGameOver)
        {
            SceneManager.LoadScene(0);
        }
    }

    private void GameOver()
    {
        _isGameOver = true;
    }
}
