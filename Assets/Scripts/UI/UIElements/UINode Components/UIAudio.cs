﻿using System;
using UnityEngine;

[Serializable]
public class UIAudio : NodeFunctionBase, IServiceUser
{
    [SerializeField] private AudioScheme _audioScheme;

    //Variables 
    private IAudioService _audioService;
    private bool _canStart;

    //Properties
    private bool UsingScheme() => _audioScheme;
    private void OnStart() => _canStart = true;
    private void SetPlayCancelAudio() => PlayCancelAudio();

    //Main
    public override void OnAwake(UiActions uiActions, Setting activeFunctions)
    {
        base.OnAwake(uiActions, activeFunctions);
        uiActions._canPlayCancelAudio += SetPlayCancelAudio;
        CanActivate = (_enabledFunctions & Setting.Audio) != 0;
        SubscribeToService();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EventLocator.SubscribeToEvent<IOnStart>(OnStart, this);
    }

    public override void RemoveFromEvents()
    {
        base.RemoveFromEvents();
        EventLocator.UnsubscribeFromEvent<IOnStart>(OnStart);
    }

    private void Play(AudioClip clip, float volume)
    {
        if(FunctionNotActive() || !_canStart) return;

        _audioService.Play(clip, volume);
    }

    protected override bool CanBeHighlighted() => !UsingScheme() ? false : _audioScheme.HighlightedClip;

    protected override bool CanBePressed() => !UsingScheme() ? false : _audioScheme.SelectedClip;

    protected override bool FunctionNotActive() => !CanActivate || !UsingScheme();

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(!CanBeHighlighted() || !pointerOver) return;
        Play(_audioScheme.HighlightedClip, _audioScheme.HighlighVolume);
    }

    protected override void SaveIsSelected(bool isSelected)
    {
        base.SaveIsSelected(isSelected);
        if(!isSelected) 
            ProcessPress();
    }

    private protected override void ProcessPress()
    {
        if(!CanBePressed()) return;
        
        if (_isSelected)
        {
            Play(_audioScheme.SelectedClip, _audioScheme.SelectedVolume);
        }
    }

    private void PlayCancelAudio()
    {
        if(FunctionNotActive()) return;
        Play(_audioScheme.CancelledClip, _audioScheme.CancelledVolume);
    }

    private protected override void ProcessDisabled()
    {
        //Do Nothing currently
    }

    public void SubscribeToService()
    {
        _audioService = ServiceLocator.GetNewService<IAudioService>(this);
        //return _audioService is null;
    }
}
