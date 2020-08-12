using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class UIAudio
{
    [SerializeField] AudioScheme _audioScheme;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Highlighted Clip")] AudioClip _soundHighlighted;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Highlighted Volume")] float _volumeHighlighted;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Selected Clip")] AudioClip _soundSelect;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Selected Volume")] float _volumeSelect;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Cancelled Clip")] AudioClip _soundCancel;
    [SerializeField] [AllowNesting] [HideIf("UsingScheme")] [Label("Cancelled Volume")] float _volumeCancel;

    //Variables 
    bool _canPlay;

    public void OnAwake(Setting setting)
    {
        _canPlay = (setting & Setting.Audio) != 0;
    }

    //Properties
    private bool UsingScheme()
    {
        return _audioScheme;
    }

    public void Play(UIEventTypes uIEventTypes)
    {
        if (!_canPlay) return;

        switch (uIEventTypes)
        {
            case UIEventTypes.Highlighted:
                if (UsingScheme())
                {
                    UIAudioManager._playAudio.Invoke(_audioScheme.HighlightedClip, _audioScheme.HighlighVolume);
                }
                else
                {
                    UIAudioManager._playAudio.Invoke(_soundHighlighted, _volumeHighlighted);
                }
                break;
            case UIEventTypes.Cancelled:
                if (UsingScheme())
                {
                    UIAudioManager._playAudio.Invoke(_audioScheme.CancelledClip, _audioScheme.CancelledVolume);
                }
                else
                {
                    UIAudioManager._playAudio.Invoke(_soundCancel, _volumeCancel);
                }
                break;
            case UIEventTypes.Selected:
                if (UsingScheme())
                {
                    UIAudioManager._playAudio.Invoke(_audioScheme.SelectedClip, _audioScheme.SelectedVolume);
                }
                else
                {
                    UIAudioManager._playAudio.Invoke(_soundSelect, _volumeSelect);
                }
                break;
        }
    }
}
