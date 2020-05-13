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

    Setting _mySetting = Setting.Audio;

    //Properties
    public AudioSource MyAudiosource { get; set; }
    public bool IsActive { get; set; } = false;

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    public void OnAwake(AudioSource audioSource)
    {
        MyAudiosource = audioSource;
    }

    public void Play(UIEventTypes uIEventTypes, Setting settingToCheck)
    {
        if (!((settingToCheck & Setting.Audio) != 0)) return;

        switch (uIEventTypes)
        {
            case UIEventTypes.Normal:
                PlayClip(_sound_Cancel, _volume_Cancel);
                break;
            case UIEventTypes.Highlighted:
                PlayClip(_sound_Highlighted, _volume_Highlighted);
                break;
            case UIEventTypes.Cancelled:
                PlayClip(_sound_Cancel, _volume_Cancel);
                break;
            case UIEventTypes.Selected:
                PlayClip(_sound_Select, _volume_Select);
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
