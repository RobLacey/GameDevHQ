using UnityEngine;

/// <summary>
/// This class handles playing UI audio. Is called by invoking the static event from within the project
/// </summary>
public class UIAudioManager : IAudioService
{
    private readonly AudioSource _myAudioSource;

    public UIAudioManager(AudioSource audioSource) => _myAudioSource = audioSource;

    public void Play(AudioClip audioClip, float volume)
    {
        _myAudioSource.clip = audioClip;
        _myAudioSource.volume = volume;
        _myAudioSource.Play();
    }
}

public interface IAudioService
{
    void Play(AudioClip audioClip, float volume);
}
