using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UIAudio
{
    public AudioClip _sound_Highlighted;
    public float _volume_Highlighted;
    public AudioClip _sound_Select;
    public float _volume_Select;
    public AudioClip _sound_Cancel;
    public float _volume_Cancel;

    //Properties
    public AudioSource MyAudiosource { get; set; }

    public bool CanPlayAudio { get; set; }

    public void OnAwake()
    {
        CanPlayAudio = false;
    }

    public void Play(UIEventTypes uIEventTypes)
    {
        switch (uIEventTypes)
        {
            case UIEventTypes.Normal:
                break;
            case UIEventTypes.Highlighted:
                PlayClip(_sound_Highlighted, _volume_Highlighted);
                break;
            case UIEventTypes.Selected:
                break;
            case UIEventTypes.Pressed:
                PlayClip(_sound_Select, _volume_Select);
                break;
            case UIEventTypes.Cancelled:
                PlayClip(_sound_Cancel, _volume_Cancel);
                break;
            default:
                break;
        }
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip)
        {
            MyAudiosource.clip = clip;
            MyAudiosource.volume = volume;
            MyAudiosource.Play();
        }
    }
}
