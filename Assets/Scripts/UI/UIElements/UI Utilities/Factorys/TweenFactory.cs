using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class TweenFactory
{
    public static TweenFactory Factory { get; set; } = new TweenFactory();

    public List<TweenBase> CreateTypes()
    {
        return null;
    }
}

[Serializable]
public class TweenData
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] private float _inTime = 1;
    [SerializeField] [AllowNesting][HideIf("UsingGlobalTime")] private float _outTime = 1;
    [SerializeField] private Ease _easeIn = Ease.Linear;
    [SerializeField] private Ease _easeOut = Ease.Linear;

    public bool UsingGlobalTime { get; set; }

    public float InTime => _inTime;

    public float OutTime => _outTime;

    public Ease EaseIn => _easeIn;

    public Ease EaseOut => _easeOut;
}

//Classes
[Serializable]
public class TweenTrigger : UnityEvent{ }


