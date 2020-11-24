using System;
using NaughtyAttributes;
using UnityEngine;

public interface IAudioSettings : IComponentSettings
{
    AudioScheme AudioScheme { get; }
}

[Serializable]
public class AudioSettings : IAudioSettings
{
    [SerializeField] private AudioScheme _audioScheme;

    public AudioScheme AudioScheme => _audioScheme;
    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        if ((functions & Setting.Audio) != 0)
        {
            return new UIAudio(this, uiNodeEvents);
        }
        return new NullFunction();
    }
}
