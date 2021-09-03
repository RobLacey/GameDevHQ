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

    //Properties
    public AudioScheme AudioScheme => _audioScheme;
    
    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        CheckForSetUpError(functions, uiNodeEvents.ReturnMasterNode);
        
        if (CanCreate(functions))
        {
            return new UIAudio(this, uiNodeEvents);
        }
        return null;
    }
    
    private bool CanCreate(Setting functions) => (functions & Setting.Audio) != 0;

    private void CheckForSetUpError(Setting functions, UINode parentNode) 
    {
        if(!CanCreate(functions)) return;
        
        if(_audioScheme.IsNull())
            throw new Exception($"No Audio Scheme assigned in Audio Settings settings for {parentNode}");
    }

}
