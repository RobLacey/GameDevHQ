using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip _LevelMusic;
    [SerializeField] AudioClip _gameOverMusic;
    [SerializeField] EventManager _Event_SetMusicVolume;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] EventManager _Event_StartLevel;

    AudioSource _myAudioSource;

    private void Awake()
    {
        _myAudioSource = GetComponent<AudioSource>();
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            StartLevel();
        }
    }

    private void OnEnable()
    {
        _Event_SetMusicVolume.AddListener((value) => _myAudioSource.volume = (float)value);
        _Event_PlayerDead.AddListener(() => GameOverMusic());
        _Event_StartLevel.AddListener(() => StartLevel());
    }

    private void StartLevel()
    {
        _myAudioSource.clip = _LevelMusic;
        _myAudioSource.Play();
    }

    private void GameOverMusic()
    {
        _myAudioSource.clip = _gameOverMusic;
        _myAudioSource.Play();
    }
}
