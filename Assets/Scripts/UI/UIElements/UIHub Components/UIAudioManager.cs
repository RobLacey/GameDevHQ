using System;
using UnityEngine;
/// <summary>
/// This class handles playing UI audio. Is called by invoking the static event from within the project
/// </summary>
public class UIAudioManager : IMono
{
    private readonly AudioSource _myAudioSource;
    public static Action<AudioClip, float> PlayAudio;

    public UIAudioManager(AudioSource audioSource)
    {
        _myAudioSource = audioSource;
        OnEnable();
    }

    public void OnEnable()
    {
        PlayAudio += Play;
    }

    public void OnDisable()
    {
        PlayAudio -= Play;
    }

    private void Play(AudioClip audioClip, float volume)
    {
        _myAudioSource.clip = audioClip;
        _myAudioSource.volume = volume;
        _myAudioSource.Play();
    }
}
