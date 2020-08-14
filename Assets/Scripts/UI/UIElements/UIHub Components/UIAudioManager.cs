using UnityEngine;

/// <summary>
/// This class handles playing UI audio. Is called by invoking the static event from within the project
/// </summary>
public class UIAudioManager
{
    private readonly AudioSource _myAudioSource;
    private readonly UIAudioEvents _uiAudioEvents = new UIAudioEvents();

    public UIAudioManager(AudioSource audioSource)
    {
        _myAudioSource = audioSource;
        OnEnable();
    }

    private void OnEnable()
    {
        _uiAudioEvents.SubscribeToPlayAudio(Play);
    }

    private void Play(AudioClip audioClip, float volume)
    {
        _myAudioSource.clip = audioClip;
        _myAudioSource.volume = volume;
        _myAudioSource.Play();
    }
}
