using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip _menuMusic;
    [SerializeField] AudioClip _gameOverMusic;
    [SerializeField] AudioClip _inGameMusic;
    [SerializeField] EventManager _Event_SetMusicVolume;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] EventManager _Event_StartLevel;

    AudioSource _myAudioSource;

    private void Awake()
    {
        _myAudioSource = GetComponent<AudioSource>();
        var musicPlayers = FindObjectsOfType<MusicPlayer>();
        if (musicPlayers.Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        _Event_SetMusicVolume.AddListener((value) => _myAudioSource.volume = (float)value);
        _Event_PlayerDead.AddListener(() => GameOverMusic());
        _Event_StartLevel.AddListener(() => StartLevel());
        SceneManager.sceneLoaded += CheckMusic;
    }

    private void CheckMusic(Scene scene, LoadSceneMode mode)
    {
        switch (scene.buildIndex)
        {
            case (0):
                StartLevel();
                break;
            case (1):
                StartLevel();
                break;
            case (2):
                StartLevel();
                break;
            case (3):
                _Event_PlayerDead.AddListener(() => GameOverMusic());
                InGameMusic();
                break;
            default:
                break;
        }
    }

    private void StartLevel()
    {
        if (_myAudioSource.clip == _menuMusic)
        {
            return;
        }
        _myAudioSource.clip = _menuMusic;
        _myAudioSource.Play();
    }

    private void GameOverMusic()
    {
        if (_myAudioSource.clip == _gameOverMusic)
        {
            return;
        }
        _myAudioSource.clip = _gameOverMusic;
        _myAudioSource.Play();
    }

    private void InGameMusic()
    {
        if (_myAudioSource.clip == _inGameMusic)
        {
            return;
        }
        _myAudioSource.clip = _inGameMusic;
        _myAudioSource.Play();
    }
}
