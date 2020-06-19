using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudioManager
{
    AudioSource _myAudioSource;

    public UIAudioManager(AudioSource audioSource)
    {
        _myAudioSource = audioSource;
        UIAudio.PlaySound += Play;
    }

    public void OnDisable()
    {
        PlayAudio -= Play;
    }

    public static event Action<AudioClip, float> PlayAudio;

    private void Play(AudioClip audioClip, float volume)
    {
        _myAudioSource.clip = audioClip;
        _myAudioSource.volume = volume;
        _myAudioSource.Play();
    }
}
