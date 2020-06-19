using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class UIAudio
{
    [SerializeField] AudioScheme _audioScheme;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Highlighted Clip")] AudioClip _sound_Highlighted;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Highlighted Volume")] float _volume_Highlighted;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Selected Clip")] AudioClip _sound_Select;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Selected Volume")] float _volume_Select;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Cancelled Clip")] AudioClip _sound_Cancel;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Cancelled Volume")] float _volume_Cancel;

    //Variables 
    public static event Action<AudioClip, float> PlaySound;
    Setting _mySetting = Setting.Audio;

    private bool UsingScheme()
    {
        return _audioScheme;
    }

    public void Play(UIEventTypes uIEventTypes, Setting settingToCheck)
    {
        if (!((settingToCheck & _mySetting) != 0)) return;

        switch (uIEventTypes)
        {
            case UIEventTypes.Normal:
                break;
            case UIEventTypes.Highlighted:
                if (UsingScheme())
                {
                    PlaySound.Invoke(_audioScheme.HighlightedClip, _audioScheme.HighlighVolume);
                }
                else
                {
                    PlaySound.Invoke(_sound_Highlighted, _volume_Highlighted);
                }
                break;
            case UIEventTypes.Cancelled:
                if (UsingScheme())
                {
                    PlaySound.Invoke(_audioScheme.CancelledClip, _audioScheme.CancelledVolume);
                }
                else
                {
                    PlaySound.Invoke(_sound_Cancel, _volume_Cancel);
                }
                break;
            case UIEventTypes.Selected:
                if (UsingScheme())
                {
                    PlaySound.Invoke(_audioScheme.SelectedClip, _audioScheme.SelectedVolume);
                }
                else
                {
                    PlaySound.Invoke(_sound_Select, _volume_Select);
                }
                break;
        }
    }
}
