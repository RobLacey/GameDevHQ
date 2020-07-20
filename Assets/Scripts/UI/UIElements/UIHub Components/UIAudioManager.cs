using System;
using UnityEngine;
/// <summary>
/// This class handles playing UI audio. Is called by invoking the static event from within the project
/// </summary>
public class UIAudioManager : IMono
{
    private readonly AudioSource _myAudioSource;
    public static Action<AudioClip, float> AudioPlay;

    public UIAudioManager(AudioSource audioSource)
    {
        _myAudioSource = audioSource;
        OnEnable();
    }

    public void OnEnable()
    {
        AudioPlay += Play;
    }

    public void OnDisable()
    {
        AudioPlay -= Play;
    }

    private void Play(AudioClip audioClip, float volume)
    {
        _myAudioSource.clip = audioClip;
        _myAudioSource.volume = volume;
        _myAudioSource.Play();
    }
}
