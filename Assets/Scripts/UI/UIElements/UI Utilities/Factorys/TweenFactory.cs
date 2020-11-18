using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class TweenFactory
{
    public static List<TweenBase> CreateTypes(TweenScheme scheme)
    {
        var activeTween = new List<TweenBase>();
        
        if (scheme.Position())
        {
            activeTween.Add(new PositionTween());
        }
        
        if (scheme.Rotation())
        {
            activeTween.Add(new RotateTween());
        }
        
        if (scheme.Scale())
        {
            activeTween.Add(new ScaleTweener());
        }
        
        if (scheme.Fade())
        {
            activeTween.Add(new FadeTween());
        }

        if (scheme.Punch())
        {
            activeTween.Add(new PunchTweener());
        }
        
        if (scheme.Shake())
        {
            activeTween.Add(new ShakeTweener());
        }
        return activeTween;
    }
    
    public static TweenBase CreateEndTween(TweenScheme scheme)
    {
        TweenBase temp = null;
        // if (scheme.Position())
        // {
        //     activeTween.Add(new PositionTween());
        // }
        //
        // if (scheme.Rotation())
        // {
        //     activeTween.Add(new RotateTween());
        // }
        //
        // if (scheme.Scale())
        // {
        //     activeTween.Add(new ScaleTweener());
        // }
        //
        // if (scheme.Fade())
        // {
        //     activeTween.Add(new FadeTween());
        // }

        if (scheme.Punch())
        {
            temp = new PunchTweener();
        }
        return temp;
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

[Serializable]
public class PunchData
{
    [SerializeField]
    [InfoBox("DOESN'T use Global Tween Time.")]
    [AllowNesting] public EffectType _punchWhen = EffectType.In;
    [SerializeField]  Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [Range(0, 1)] float _elasticity = 0.5f;
    [SerializeField] [Range(1, 10)] int _vibrato = 5;

    public EffectType PunchWhen => _punchWhen;

    public Vector3 Strength => _strength;

    public float Duration => _duration;

    public float Elasticity => _elasticity;

    public int Vibrato => _vibrato;
}

[Serializable]
public class ShakeData
{
    [SerializeField]
    [InfoBox("DOESN'T use Global Tween Time.")]
    [AllowNesting] public EffectType _shakeWhen = EffectType.In;
    [SerializeField] Vector3 _strength = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [Range(1, 10)] int _vibrato = 10;
    [SerializeField] [Range(0, 90)] float _randomness = 45f;
    [SerializeField] bool _fadeOut = true;

    public EffectType ShakeWhen => _shakeWhen;

    public Vector3 Strength => _strength;

    public float Duration => _duration;

    public int Vibrato => _vibrato;

    public float Randomness => _randomness;

    public bool FadeOut => _fadeOut;
}


