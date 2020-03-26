using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] EventManager _Event_StartLevel;
    [SerializeField] EventManager _Event_SetMusicVolume;
    [SerializeField] EventManager _Event_NoHighScore;
    [SerializeField] EventManager _Event_StartFadeOut;

    public bool IsGameOver { get; set; }

    private void Awake()
    {
        IsGameOver = false;
    }

    private void OnEnable()
    {
        _Event_NoHighScore.AddListener(() => GameOver());
    }

    private void Start()
    {
        _Event_SetMusicVolume.Invoke(PlayerPrefs.GetFloat(PlayerSettings._musicSettings));
        //TODO Add Audio Mixer for SFX to set level
        //TODO Add Pause Menu to Change Music and SFX too
        _Event_StartLevel.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Quit");
        }

        if (Input.GetKeyDown(KeyCode.R) && IsGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.M) && IsGameOver)
        {
            _Event_StartFadeOut.Invoke();
        }
    }

    private void GameOver()
    {
        IsGameOver = true;
    }
}
