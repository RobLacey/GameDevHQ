using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TweenEvents
{
    [SerializeField] 
    private TweenTrigger _inTweenEventStart;
    [SerializeField] 
    private TweenTrigger _inTweenEventEnd;
    [SerializeField] 
    private TweenTrigger _outTweenEventStart;
    [SerializeField] 
    private TweenTrigger _outTweenEventEnd;

    public TweenTrigger InTweenEventStart => _inTweenEventStart;

    public TweenTrigger InTweenEventEnd => _inTweenEventEnd;

    public TweenTrigger OutTweenEventStart => _outTweenEventStart;

    public TweenTrigger OutTweenEventEnd => _outTweenEventEnd;
}

//Classes
[Serializable]
public class TweenTrigger : UnityEvent{ }

