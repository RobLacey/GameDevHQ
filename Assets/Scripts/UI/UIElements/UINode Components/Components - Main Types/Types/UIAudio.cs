using EZ.Service;
using UnityEngine;

public class UIAudio : NodeFunctionBase
{
    public UIAudio(IAudioSettings settings, IUiEvents uiEvents): base(uiEvents)
    {
        _uiEvents = uiEvents;
        _audioScheme = settings.AudioScheme;
    }

    //Variables 
    private AudioScheme _audioScheme;
    private IAudioService _audioService;
    private bool _audioIsMute;

    //Properties, Getters & Setters
    private bool AudioIsMute => !_myDataHub.SceneStarted || _audioIsMute;
    private void AudioIsMuted() => _audioIsMute = true;
    protected override bool CanBeHighlighted() => _audioScheme.HighlightedClip;
    protected override bool CanBePressed() => _audioScheme.SelectedClip;
    private bool HasDisabledSound() => _audioScheme.DisabledClip;
    private bool HasCancelSound() => _audioScheme.CancelledClip;
    
    //Main
     public override void UseEZServiceLocator()
     {
         base.UseEZServiceLocator();
         _audioService = EZService.Locator.Get<IAudioService>(this);
     }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        _uiEvents.MuteAudio += AudioIsMuted;
        InputEvents.Do.Subscribe<IHotKeyPressed>(HotKeyPressed);
        _audioIsMute = true;
    }

    protected override void LateStartSetUp()
    {
        base.LateStartSetUp();
        if(MyHubDataIsNull) return;

        _audioIsMute = false;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UnObserveEvents();
        _audioService = null;
    }

    public override void UnObserveEvents()
    {
        base.UnObserveEvents();
        InputEvents.Do.Unsubscribe<IHotKeyPressed>(HotKeyPressed);
        _uiEvents.MuteAudio -= AudioIsMuted;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnObserveEvents();
        _audioService = null;
        _audioScheme = null;
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if (IsAudioMuted()) return;
        if (pointerOver)
            PlayHighlightedAudio();
    }

    protected override void SaveIsSelected(bool isSelected)
    {
        if(!CanBePressed()) return;
        if (IsAudioMuted()) return;
        
        if (!isSelected)
        {
            PlayCancelAudio();
        }
        else
        {
            PlaySelectedAudio();
        }
    }

    private bool IsAudioMuted()
    {
        if (AudioIsMute)
        {
            _audioIsMute = false;
            return true;
        }
        return false;
    }

    private protected override void ProcessPress() { }

    private bool IsDisabledCheckForDisabledSound()
    {
        if (!_isDisabled || !HasDisabledSound()) return false;
        
        _audioService.PlayDisabled(_audioScheme.DisabledClip, _audioScheme.DisabledVolume);
        return true;
    }

    private void PlayCancelAudio()
    {
        if(FunctionNotActive() || !HasCancelSound()) return;
        _audioService.PlayCancel(_audioScheme.CancelledClip, _audioScheme.CancelledVolume);
    }
    
    private void PlaySelectedAudio()
    {
        if(IsDisabledCheckForDisabledSound()) return;
        if(FunctionNotActive() || !CanBePressed()) return;
        _audioService.PlaySelect(_audioScheme.SelectedClip, _audioScheme.SelectedVolume);
    }
    private void PlayHighlightedAudio()
    {
        if(IsDisabledCheckForDisabledSound()) return;
        if(FunctionNotActive() || !CanBeHighlighted()) return;
        _audioService.PlayHighlighted(_audioScheme.HighlightedClip, _audioScheme.HighlightedVolume);
    }

    private protected override void ProcessDisabled() { }

    private void HotKeyPressed(IHotKeyPressed args)
    {
        if (ReferenceEquals(args.ParentNode, _uiEvents.ReturnMasterNode))
            PlaySelectedAudio();
    }
}
