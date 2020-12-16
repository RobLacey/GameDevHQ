using System;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "TweenScheme", menuName = "UIElements Schemes / New Tween Scheme")]
public class TweenScheme: ScriptableObject
{
    [Header("Tween Settings", order = 1)] [HorizontalLine(1, EColor.Blue , order = 2)]
    [SerializeField] 
    [Space(15f)] 
    private TweenStyle _positionTween = TweenStyle.NoTween;
    [SerializeField] 
    private TweenStyle _rotationTween = TweenStyle.NoTween;
    [SerializeField] 
    private TweenStyle _fadeTween = TweenStyle.NoTween;
    [SerializeField] 
    private TweenStyle _scaleTween = TweenStyle.NoTween;
    [SerializeField] [DisableIf("Shake")]
    private TweenStyle _punchTween = TweenStyle.NoTween;
    [SerializeField] [DisableIf("Punch")]
    private TweenStyle _shakeTween = TweenStyle.NoTween;
    
    [Header("Tween Data", order = 1)] 
    [HorizontalLine(1, EColor.Blue , order = 2)]
    
    [SerializeField]
    private IsActive _useGlobalTime = IsActive.No;
    [SerializeField] 
    [ShowIf("GlobalTime")]
    private float _globalInTime = 1;
    [SerializeField] 
    [ShowIf("GlobalTime")]
    private float _globalOutTime = 1;
    
    [SerializeField] 
    [EnableIf("Position")] 
    private TweenData _positionData;
    [SerializeField]  
    [EnableIf("Rotation")] 
    private TweenData _rotationData;
    [SerializeField]  
    [EnableIf("Scale")] 
    private TweenData _scaleData;
    [SerializeField]  
    [EnableIf("Fade")] 
    private TweenData _fadeData;
    [SerializeField]  
    [EnableIf("Shake")] 
    private ShakeData _shakeData;
    [SerializeField]  
    [EnableIf("Punch")] 
    private PunchData _punchData;

    //Events
    private Action _onChange;

    private void OnValidate() => _onChange?.Invoke();

    public void Subscribe(Action listener) => _onChange += listener;

    public void Unsubscribe(Action listener) => _onChange -= listener;

    public TweenStyle PositionTween => _positionTween;

    public TweenStyle RotationTween => _rotationTween;

    public TweenStyle FadeTween => _fadeTween;

    public TweenStyle ScaleTween => _scaleTween;

    public TweenStyle PunchTween => _punchTween;
    public TweenStyle ShakeTween => _shakeTween;

    public TweenData PositionData => _positionData;

    public TweenData RotationData => _rotationData;

    public TweenData ScaleData => _scaleData;

    public TweenData FadeData => _fadeData;
    public PunchData PunchData => _punchData;
    public ShakeData ShakeData => _shakeData;
    
    public float SetPositionTime(TweenType tweenType)
    {
        return tweenType == TweenType.In ? SetInTime(_positionData) : SetOutTime(_positionData);
    }
    public float SetRotationTime(TweenType tweenType)
    {
        return tweenType == TweenType.In ? SetInTime(_rotationData) : SetOutTime(_rotationData);
    }
    public float SetScaleTime(TweenType tweenType)
    {
        return tweenType == TweenType.In ? SetInTime(_scaleData) : SetOutTime(_scaleData);
    }
    public float SetFadeTime(TweenType tweenType)
    {
        return tweenType == TweenType.In ? SetInTime(_fadeData) : SetOutTime(_fadeData);
    }

    private float SetInTime(TweenData tween) => _useGlobalTime == IsActive.Yes ? _globalInTime : tween.InTime;

    private float SetOutTime(TweenData tween) => _useGlobalTime == IsActive.Yes ? _globalOutTime : tween.OutTime;


    public bool GlobalTime() //**
    {
        if (_useGlobalTime == IsActive.Yes)
        {
            _positionData.UsingGlobalTime = true;
            _rotationData.UsingGlobalTime = true;
            _scaleData.UsingGlobalTime = true;
            _fadeData.UsingGlobalTime = true;
            return true;
        }

        _positionData.UsingGlobalTime = false;
        _rotationData.UsingGlobalTime = false;
        _scaleData.UsingGlobalTime = false;
        _fadeData.UsingGlobalTime = false;
        return false;
    }

    public bool Position() => _positionTween != TweenStyle.NoTween;

    public bool Rotation() => _rotationTween != TweenStyle.NoTween;

    public bool Scale() => _scaleTween != TweenStyle.NoTween;
    public bool Fade() => _fadeTween != TweenStyle.NoTween;
    public bool Punch() => _punchTween != TweenStyle.NoTween;

    public bool Shake() => _shakeTween != TweenStyle.NoTween; 
}

