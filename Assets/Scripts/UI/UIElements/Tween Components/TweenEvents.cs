using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TweenEvents
{
    [SerializeField] 
    private TweenTrigger _inTweenEvent_Start;
    [SerializeField] 
    private TweenTrigger _inTweenEvent_End;
    [SerializeField] 
    private TweenTrigger _outTweenEvent_Start;
    [SerializeField] 
    private TweenTrigger _outTweenEvent_End;

    public TweenTrigger InTweenEvent_Start => _inTweenEvent_Start;

    public TweenTrigger InTweenEvent_End => _inTweenEvent_End;

    public TweenTrigger OutTweenEvent_Start => _outTweenEvent_Start;

    public TweenTrigger OutTweenEvent_End => _outTweenEvent_End;
}

//Classes
[Serializable]
public class TweenTrigger : UnityEvent{ }

