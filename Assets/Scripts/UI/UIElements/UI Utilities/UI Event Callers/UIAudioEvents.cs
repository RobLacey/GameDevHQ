using System;
using UnityEngine;

public class UIAudioEvents : UiEventCaller
{
    private CustomEventHandler<AudioClip, float> _playAudio;
    
    protected override void OnExit()
    {
        UIAudio.PlayAudio -= _playAudio.Event;
    }
    
    public void SubscribeToPlayAudio(Action<AudioClip, float> subscriber)
    {
        _playAudio = new CustomEventHandler<AudioClip, float>();
        UIAudio.PlayAudio += _playAudio.Add(subscriber);
    }

}
