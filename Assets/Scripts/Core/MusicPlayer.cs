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
    [SerializeField] float _musicFadeIn = 5f;

    //Variables
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
        _Event_SetMusicVolume.AddListener((value) => SetAudio(value), this);
        _Event_PlayerDead.AddListener(() => GameOverMusic(), this);
        SceneManager.sceneLoaded += CheckMusic;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= CheckMusic;
    }

    private void SetAudio(object newVolume)
    {
        _myAudioSource.volume = (float)newVolume;
    }

    private void CheckMusic(Scene scene, LoadSceneMode mode)
    {
        switch (scene.buildIndex)
        {
            case (0):
                StartLevel();
                break;
            case (1):
                _Event_SetMusicVolume.AddListener((value) => SetAudio(value), this);
                StartLevel();
                break;
            case (2):
                _Event_SetMusicVolume.AddListener((value) => SetAudio(value), this);
                StartLevel();
                break;
            case (3):
                _Event_SetMusicVolume.AddListener((value) => StartMusic(value), this);
                _Event_PlayerDead.AddListener(() => GameOverMusic(), this);
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

    private void StartMusic(object volume)
    {
        _myAudioSource.volume = (float)volume;
        StartCoroutine(IncreaseVolume());
    }

    IEnumerator IncreaseVolume()
    {
        float timer = 0;
        float perc = 0;
        float endvolume = _myAudioSource.volume;

        _myAudioSource.volume = 0;

        while (_myAudioSource.volume < endvolume)
        {
            timer += Time.deltaTime;
            perc = timer / _musicFadeIn;
            _myAudioSource.volume = Mathf.Lerp( 0, endvolume, perc);
            yield return null;
        }
    }
}
