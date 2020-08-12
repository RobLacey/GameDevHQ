using System;
using UnityEngine;
/// <summary>
/// This class handles playing UI audio. Is called by invoking the static event from within the project
/// </summary>
public class UIAudioManager
{
    private readonly AudioSource _myAudioSource;
    public static Action<AudioClip, float> _playAudio;

    public UIAudioManager(AudioSource audioSource)
    {
        _myAudioSource = audioSource;
        Application.quitting += OnDisable;
        OnEnable();
    }

    private void OnEnable()
    {
        _playAudio += Play;
    }

    private void OnDisable()
    {
        _playAudio -= Play;
    }

    private void Play(AudioClip audioClip, float volume)
    {
        _myAudioSource.clip = audioClip;
        _myAudioSource.volume = volume;
        _myAudioSource.Play();
    }
}
