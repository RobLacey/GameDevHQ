using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudioManager
{
    AudioSource _myAudioSource;
    public static Action<AudioClip, float> AudioPlay;

    public UIAudioManager(AudioSource audioSource)
    {
        _myAudioSource = audioSource;
        AudioPlay += Play;
    }

    public void OnDisable()
    {
        AudioPlay -= Play;
    }

    public static event Action<AudioClip, float> PlayAudio;

    private void Play(AudioClip audioClip, float volume)
    {
        _myAudioSource.clip = audioClip;
        _myAudioSource.volume = volume;
        _myAudioSource.Play();
    }
}
