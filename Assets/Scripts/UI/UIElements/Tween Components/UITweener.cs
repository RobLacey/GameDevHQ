using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics.CodeAnalysis;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine.Events;

[SuppressMessage("ReSharper", "IdentifierTypo")]
public partial class UITweener : MonoBehaviour
{
    [SerializeField] 
    [ReorderableList] [Label("List Of Objects To Tween")]
    private List<BuildTweenData> _buildObjectsList = new List<BuildTweenData>();

    [SerializeField] [Expandable] private TweenScheme _scheme;
    
    // [Header("Time Settings", order = 1)] [HorizontalLine(1, EColor.Blue , order = 2)]
    // [SerializeField]
    // private IsActive _useGlobalTime = IsActive.No;
    // [SerializeField] 
    // [ShowIf("GlobalTime")]
    // private float _globalInTime = 1;
    // [SerializeField] 
    // [ShowIf("GlobalTime")]
    // private float _globalOutTime = 1;
    //
    // [Header("Tween Settings", order = 1)] [HorizontalLine(1, EColor.Blue , order = 2)]
    // [SerializeField] 
    // [Space(15f)] 
    // private TweenStyle _positionTween = TweenStyle.NoTween;
    // [SerializeField] 
    // private TweenStyle _rotationTween = TweenStyle.NoTween;
    // [SerializeField] 
    // [Label("Fade (Canvas Group Only)")] private TweenStyle _canvasGroupFade = TweenStyle.NoTween;
    // [SerializeField] 
    // [Label("Scale Tween")]
    //  private TweenStyle _scaleTransition = TweenStyle.NoTween;
    // [SerializeField] 
    // [Label("Shake or Punch Tween")] private PunchShakeTween _punchShakeTween = PunchShakeTween.NoTween;
    // [SerializeField] 
    // [Label("Shake/Punch At End (In Only)")] private IsActive _shakeOrPunchAtEnd = IsActive.No;
    //
    // [Header("Event Settings")] [HorizontalLine(1, EColor.Blue , order = 2)]
    // [SerializeField] 
    // private IsActive _addTweenEventTriggers = IsActive.No;
    // [SerializeField] 
    // [ShowIf("AddTweenEvent")] [Label("Event At After Start/Mid-Point of Tween")] 
    // private TweenTrigger _middleOfTweenAction;
    // [SerializeField] 
    // [ShowIf("AddTweenEvent")] [Label("Event At End of Tween")]
    // private TweenTrigger _endOfTweenAction;
    //
    // [SerializeField] 
    // [Label("Settings")] [ShowIf("Position")] [BoxGroup("Position Tween")]
    private PositionTween _posTween = new PositionTween();
    // [SerializeField] 
    // [Label("Settings")] [ShowIf("Rotation")] [BoxGroup("Rotation Tween")]
    private RotateTween _rotateTween = new RotateTween();
    // [SerializeField] 
    // [Label("Settings")] [ShowIf("Scale")] [BoxGroup("Scale Tween")]
    private ScaleTweener _scaleTween = new ScaleTweener();
    // [SerializeField] 
    // [Label("Settings")] [ShowIf("Punch")] [BoxGroup("Punch Tween")]
    private PunchTweener _punchTween = new PunchTweener();
    // [SerializeField] 
    // [Label("Settings")] [ShowIf("Shake")] [BoxGroup("Shake Tween")]
    private ShakeTweener _shakeTween = new ShakeTweener();
    // [SerializeField]
    // [Label("Settings")] [ShowIf("Fade")] [BoxGroup("Fade Tween")]
    private FadeTween _fadeTween = new FadeTween();

    //Variables
    private int _counter, _effectCounter;
    private IsActive _doEffectOnInTween = IsActive.No;

    //Delegates
    private Action _finishedTweenCallback;
    private bool _hasScheme;
    private TweenScheme _lastTweenScheme;

    // //Classes
    // [Serializable]
    // public class TweenTrigger : UnityEvent{ }

    public void Awake() 
    {
        if(_buildObjectsList.Count == 0) return;
        SetUpGenericTween(_posTween, _scheme.PositionTween);
        SetUpGenericTween(_rotateTween, _scheme.RotationTween);
        SetUpGenericTween(_scaleTween, _scheme.ScaleTween);
        SetUpGenericTween(_fadeTween, _scheme.FadeTween);
        SetUpPunchShakeTween();
    }

    private void OnValidate()
    {
        foreach (var element in _buildObjectsList)
        {
            element.SetElement();
        }
        
        if(_scheme is null && _hasScheme)
        {
            _hasScheme = false;
            if(_lastTweenScheme != null) 
                _lastTweenScheme.Unsubscribe(CheckingSettings);
            _lastTweenScheme = null;
            IsRotationSet(TweenStyle.NoTween);
            IsPositionSet(TweenStyle.NoTween);
            IsScaleSet(TweenStyle.NoTween);

            return;
        }

        if (_scheme && !_hasScheme)
        {
            _hasScheme = true;
            _lastTweenScheme = _scheme;
            _lastTweenScheme.Subscribe(CheckingSettings);
        }
        CheckingSettings();
    }

    private void CheckingSettings()
    {
        if(_scheme is null) return;
        IsRotationSet(_scheme.RotationTween);
        IsPositionSet(_scheme.PositionTween);
        IsScaleSet(_scheme.ScaleTween);
    }

    private void SetUpGenericTween(TweenBase tweenBase, TweenStyle tweenStyle )
    {
        if (tweenStyle == TweenStyle.NoTween) return;
        _counter++;
        tweenBase.SetUpTweens(_buildObjectsList, InTweenEndEffect);
    }
    
    //TODO Check end effect call is only called once

    private void SetUpPunchShakeTween()
    {
        if (_scheme.PunchOrShakeTween == PunchShakeTween.Punch)
        {
            if (_scheme.ShakeOrPunchAtEnd == IsActive.No) 
                _counter++;
            _punchTween.SetUpPunchTween(_buildObjectsList);
        }
        else if (_scheme.PunchOrShakeTween == PunchShakeTween.Shake)
        {
            if (_scheme.ShakeOrPunchAtEnd == IsActive.No) 
                _counter++;
            _shakeTween.SetUpShakeTween(_buildObjectsList);
        }
    }

    public void ActivateTweens(Action callBack)
    {    
        _finishedTweenCallback = callBack;
        if (IfTweenCounterIsZero_In()) return;
        SetTweensUp(/*_scheme.SetInTime(), */TweenType.In, InTweenEndAction, IsActive.Yes);
    }

    private bool IfTweenCounterIsZero_In()
    {
        if (_counter > 0) return false;
        InTweenEndAction();
        return true;
    }

    public void DeactivateTweens(Action callBack)
    {
        _finishedTweenCallback = callBack;
        if (IfTweenCounterIsZero_Out()) return;
        SetTweensUp(/*_globalOutTime, */TweenType.Out, OutTweenEndAction, IsActive.No);
    }

    private bool IfTweenCounterIsZero_Out()
    {
        if (_counter > 0) return false;
        OutTweenEndAction();
        _scheme.EndOfTweenAction?.Invoke();
        return true;
    }

    private void SetTweensUp(/*float globalTime, */TweenType tweenType, 
                             TweenCallback callback, IsActive allowInTweenEffect)
    {
        StopAllCoroutines();
        _effectCounter = _counter;
        _doEffectOnInTween = allowInTweenEffect;
        DoTweens(/*TimeToUseForTween(globalTime),*/ tweenType, callback);
    }

    private void DoTweens(/*loat tweenTime, */TweenType tweenType, TweenCallback endaction)
    {
        _posTween.StartTween(_scheme.PositionTween, _scheme.SetPositionTime(tweenType), tweenType, endaction);
        _scaleTween.StartTween(_scheme.ScaleTween, _scheme.SetScaleTime(tweenType), tweenType, endaction);
        _rotateTween.StartTween(_scheme.RotationTween, _scheme.SetRotationTime(tweenType), tweenType, endaction);
        _fadeTween.StartTween(_scheme.FadeTween, _scheme.SetFadeTime(tweenType), tweenType, endaction);

        if (_scheme.ShakeOrPunchAtEnd != IsActive.No) return;
        _punchTween.DoPunch(_scheme.PunchOrShakeTween, tweenType, endaction);
        _shakeTween.DoShake(_scheme.PunchOrShakeTween, tweenType, endaction);
    }
    
    private void InTweenEndAction()
    {
        _effectCounter--;
        if (_effectCounter > 0) return;
        _scheme.MiddleOfTweenAction?.Invoke();
        _finishedTweenCallback?.Invoke();
    }

    private void OutTweenEndAction()
    {
        _effectCounter--;
        if (_effectCounter > 0) return;
        _scheme.EndOfTweenAction?.Invoke();
        _finishedTweenCallback?.Invoke();
    }
    
   // private float TimeToUseForTween(float timeToUse) => _useGlobalTime == IsActive.Yes ? timeToUse : 0;

    private void InTweenEndEffect(RectTransform uiObject = null)
    {
        if (_scheme.ShakeOrPunchAtEnd == IsActive.No) return;
        switch (_scheme.PunchOrShakeTween)
        {
            case PunchShakeTween.Shake:
                _shakeTween.EndEffect(uiObject, _doEffectOnInTween);
                break;
            case PunchShakeTween.Punch:
                _punchTween.EndEffect(uiObject, _doEffectOnInTween);
                break;
        }
    }
}