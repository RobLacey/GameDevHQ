using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// This class handles playing UI audio. Is called by invoking the static event from within the project
/// </summary>
public class UIAudioManager : IAudioService
{
    private readonly AudioSource _myAudioSource;
    private bool IsPlayingSelected { get; set; }
    private bool IsPlayingCancel { get; set; }

    
    public UIAudioManager(AudioSource audioSource) => _myAudioSource = audioSource;

    private void Play(AudioClip audioClip, float volume)
    {
        _myAudioSource.clip = audioClip;
        _myAudioSource.volume = volume;
        _myAudioSource.Play();
    }

    public void PlaySelect(AudioClip audioClip, float volume)
    {
       // Debug.Log("Selected");

        IsPlayingSelected = true;
        Play(audioClip, volume);
        StaticCoroutine.StartCoroutine(Timer(audioClip.length, ()=> IsPlayingSelected = false));
    }

    public void PlayCancel(AudioClip audioClip, float volume)
    {
        if(IsPlayingSelected) return;
//        Debug.Log("Cancel");
        IsPlayingCancel = true;
        Play(audioClip, volume);
        StaticCoroutine.StartCoroutine(Timer(audioClip.length, ()=> IsPlayingCancel = false));
    }

    public void PlayHighlighted(AudioClip audioClip, float volume)
    {
        if(IsPlayingSelected || IsPlayingCancel) return;
        Play(audioClip, volume);
    }

    private static IEnumerator Timer(float time, Action condition)
    {
        yield return new WaitForSeconds(time);
        condition.Invoke();
    }

    public void PlayDisabled(AudioClip audioClip, float volume) => Play(audioClip, volume);
}

public interface IAudioService
{
    void PlaySelect(AudioClip audioClip, float volume);
    void PlayCancel(AudioClip audioClip, float volume);
    void PlayHighlighted(AudioClip audioClip, float volume);
    void PlayDisabled(AudioClip audioClip, float volume);
}
