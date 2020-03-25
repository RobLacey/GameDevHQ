using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField] AudioClip _gameOverVoice;
    [SerializeField] AudioClip _countDownVoice;
    [SerializeField] EventManager _Event_PlayerDead;
    [SerializeField] EventManager _Event_StartLevel;

    AudioSource _myAudioSource;

    private void Awake()
    {
        _myAudioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _Event_PlayerDead.AddListener(() => GameOverMusic());
        _Event_StartLevel.AddListener(() => StartLevel());
    }

    private void StartLevel()
    {
        _myAudioSource.clip = _countDownVoice;
        _myAudioSource.Play();
    }

    private void GameOverMusic()
    {
        _myAudioSource.clip = _gameOverVoice;
        _myAudioSource.volume = 1f;
        _myAudioSource.Play();
    }

}
