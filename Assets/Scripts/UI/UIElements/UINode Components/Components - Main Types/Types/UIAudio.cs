﻿using UnityEngine;

public class UIAudio : NodeFunctionBase, IEServUser
{
    public UIAudio(IAudioSettings settings, IUiEvents uiEvents)
    {
        _uiEvents = uiEvents;
        _audioScheme = settings.AudioScheme;
        CanActivate = true;
        OnAwake(uiEvents);
    }

    //Variables 
    private readonly AudioScheme _audioScheme;
    private IAudioService _audioService;
    private bool _audioIsMute;

    //Properties
    private bool UsingScheme() => _audioScheme;
    private void AudioIsMuted() => _audioIsMute = true;

    //Main
    protected sealed override void OnAwake(IUiEvents uiEvents)
    {
        base.OnAwake(uiEvents);
        _uiEvents.MuteAudio += AudioIsMuted;
        UseEServLocator();
    }

     public void UseEServLocator() => _audioService = EServ.Locator.Get<IAudioService>(this);

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<IHotKeyPressed>(HotKeyPressed);
    }

    public override void RemoveEvents()
    {
        base.RemoveEvents();
        EVent.Do.Unsubscribe<IHotKeyPressed>(HotKeyPressed);
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