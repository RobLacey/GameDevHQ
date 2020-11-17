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
    [Label("Fade (Canvas Group Only)")] private TweenStyle _fadeTween = TweenStyle.NoTween;
    [SerializeField] 
    [Label("Scale Tween")]
    private TweenStyle _scaleTween = TweenStyle.NoTween;
    [SerializeField] 
    [Label("Shake or Punch Tween")] private PunchShakeTween _punchShakeTween = PunchShakeTween.NoTween;
    [SerializeField] 
    [Label("Shake/Punch At End (In Only)")] private IsActive _shakeOrPunchAtEnd = IsActive.No;

    [SerializeField] /*[ShowIf("Position")]*/ 
    private TweenData _positionData;
    [SerializeField] /*[ShowIf("Rotation")] */
    private TweenData _rotationData;
    [SerializeField] 
    private TweenData _scaleData;
    [SerializeField] /*[ShowIf("Fade")] */
    private TweenData _fadeData;

    [Header("Event Settings")] [HorizontalLine(1, EColor.Blue , order = 3)]
    [SerializeField] 
    private IsActive _addTweenEventTriggers = IsActive.No;
    [SerializeField] 
    [ShowIf("AddTweenEvent")] [Label("Event At After Start/Mid-Point of Tween")] 
    private TweenTrigger _middleOfTweenAction;
    [SerializeField] 
    [ShowIf("AddTweenEvent")] [Label("Event At End of Tween")]
    private TweenTrigger _endOfTweenAction;

    [Header("Time Settings", order = 1)] [HorizontalLine(1, EColor.Blue , order = 4)]
    [SerializeField]
    private IsActive _useGlobalTime = IsActive.No;
    [SerializeField] 
    [ShowIf("GlobalTime")]
    private float _globalInTime = 1;
    [SerializeField] 
    [ShowIf("GlobalTime")]
    private float _globalOutTime = 1;

    //Events
    private Action Change;

    private void OnValidate() => Change?.Invoke();

    public void Subscribe(Action listener) => Change += listener;

    public void Unsubscribe(Action listener) => Change -= listener;

    public TweenStyle PositionTween => _positionTween;

    public TweenStyle RotationTween => _rotationTween;

    public TweenStyle FadeTween => _fadeTween;

    public TweenStyle ScaleTween => _scaleTween;

    public PunchShakeTween PunchOrShakeTween => _punchShakeTween;

    public IsActive ShakeOrPunchAtEnd => _shakeOrPunchAtEnd;

    public TweenData PositionData => _positionData;

    public TweenData RotationData => _rotationData;

    public TweenData ScaleData => _scaleData;

    public TweenData FadeData => _fadeData;
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

    public TweenTrigger MiddleOfTweenAction => _middleOfTweenAction;

    public TweenTrigger EndOfTweenAction => _endOfTweenAction;

    private float SetInTime(TweenData tween) => _useGlobalTime == IsActive.Yes ? _globalInTime : tween.InTime;

    private float SetOutTime(TweenData tween) => _useGlobalTime == IsActive.Yes ? _globalInTime : tween.OutTime;


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
    public bool Punch() => _punchShakeTween == PunchShakeTween.Punch;

    public bool Shake() => _punchShakeTween == PunchShakeTween.Shake; 
}

