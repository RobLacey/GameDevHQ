using EZ.Service;
using UnityEngine;

public class UIAudio : NodeFunctionBase
{
    public UIAudio(IAudioSettings settings, IUiEvents uiEvents): base(uiEvents)
    {
        _uiEvents = uiEvents;
        _audioScheme = settings.AudioScheme;
        CanActivate = true;
    }

    //Variables 
    private AudioScheme _audioScheme;
    private IAudioService _audioService;
    private bool _audioIsMute;

    //Properties
    private bool UsingScheme() => _audioScheme;
    private void AudioIsMuted() => _audioIsMute = true;
    private void CanStart(IOnStart args) => _audioIsMute = false;


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
        HistoryEvents.Do.Subscribe<IOnStart>(CanStart);
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

    protected override void UnObserveEvents()
    {
        base.UnObserveEvents();
        InputEvents.Do.Unsubscribe<IHotKeyPressed>(HotKeyPressed);
        HistoryEvents.Do.Unsubscribe<IOnStart>(CanStart);
        _uiEvents.MuteAudio -= AudioIsMuted;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnObserveEvents();
        _audioService = null;
        _audioScheme = null;
    }

    protected override bool CanBeHighlighted() => !UsingScheme() ? false : _audioScheme.HighlightedClip;

    protected override bool CanBePressed() => !UsingScheme() ? false : _audioScheme.SelectedClip;

    private bool HasDisabledSound() => !UsingScheme() ? false : _audioScheme.DisabledClip;

    private bool HasCancelSound() => !UsingScheme() ? false : _audioScheme.CancelledClip;

    protected override bool FunctionNotActive() => !CanActivate || !UsingScheme();

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
        if (_audioIsMute)
        {
            _audioIsMute = false;
            return true;
        }
        return false;
    }

    private protected override void ProcessPress() { }

    private bool IsDisabled()
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
        if(FunctionNotActive() || !CanBePressed()) return;
        if(IsDisabled()) return;
        _audioService.PlaySelect(_audioScheme.SelectedClip, _audioScheme.SelectedVolume);
    }
    private void PlayHighlightedAudio()
    {
        if(FunctionNotActive() || !CanBeHighlighted()) return;
        if(IsDisabled()) return;
        _audioService.PlayHighlighted(_audioScheme.HighlightedClip, _audioScheme.HighlightedVolume);
    }

    private protected override void ProcessDisabled() { }

    private void HotKeyPressed(IHotKeyPressed args)
    {
        if (ReferenceEquals(args.ParentNode, _uiEvents.ReturnMasterNode))
            PlaySelectedAudio();
    }
}
