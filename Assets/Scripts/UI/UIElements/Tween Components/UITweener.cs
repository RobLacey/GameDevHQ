﻿using System.Collections;
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
    [SerializeField] [ReorderableList] [Label("List Of Objects To Apply Effects To")] 
    List<TweenSettings> _buildObjectsList = new List<TweenSettings>();
    [InfoBox("Add PARENT to apply effects as a whole or add each CHILD for a build effect. List is DRAG & DROP orderable", order = 0)]
    [Header("Effect Tween Settings", order = 1)] [HorizontalLine(4, color: EColor.Blue, order = 2)]
    [Header("Time Settings", order = 3)]
    [SerializeField] IsActive _useGlobalTime = IsActive.No;
    [SerializeField] [ShowIf("GlobalTime")] float _globalInTime = 1;
    [SerializeField] [ShowIf("GlobalTime")] float _globalOutTime = 1;
    [Header("Tween Settings")]
    [SerializeField] public PositionTweenType _positionTween = PositionTweenType.NoTween;
    [SerializeField] public RotationTweenType _rotationTween = RotationTweenType.NoTween;
    [SerializeField] [Label("Fade (Canvas Group Only)")] public FadeTween _canvasGroupFade = FadeTween.NoTween;
    [SerializeField] [Label("Scale Tween")] public ScaleTween _scaleTransition = ScaleTween.NoTween;
    [SerializeField] [Label("Shake or Punch Tween")] public PunchShakeTween _punchShakeTween = PunchShakeTween.NoTween;
    [SerializeField] [Label("Shake/Punch At End (In Only)")] IsActive _shakeOrPunchAtEnd = IsActive.No;
    [SerializeField] IsActive _addTweenEventTriggers = IsActive.No;
    [SerializeField] [ShowIf("AddTweenEvent")] [Label("Event At After Start/Mid-Point of Tween")] 
    TweenTrigger _middleOfTweenAction;
    [SerializeField] [ShowIf("AddTweenEvent")] [Label("Event At End of Tween")] 
    TweenTrigger _endOfTweenAction;
    [SerializeField] [Label("Settings")] [ShowIf("Position")] [BoxGroup("Position Tween")]
    private PositionTween _posTween;
    [SerializeField] [Label("Settings")] [ShowIf("Rotation")] [BoxGroup("Rotation Tween")]
    private RotateTween _rotateTween;
    [SerializeField] [Label("Settings")] [ShowIf("Scale")] [BoxGroup("Scale Tween")] 
    ScaleTweener _scaleTween;
    [SerializeField]  [Label("Settings")] [ShowIf("Punch")] [BoxGroup("Punch Tween")] 
    PunchTweener _punchTween;
    [SerializeField] [Label("Settings")] [ShowIf("Shake")] [BoxGroup("Shake Tween")] 
    ShakeTweener _shakeTween;
    [SerializeField] [Label("Settings")] [ShowIf("Fade")] [BoxGroup("Fade Tween")] 
    FadeTweener _fadeTween;

    //Variables
    private int _counter;
    private int _effectCounter;
    private IsActive _doEffectOnInTween = IsActive.No;

    //Delegates
    private Action _finishedTweenCallback;

    //Classes
    [Serializable]
    public class TweenTrigger : UnityEvent{ }

    public void OnAwake() 
    {
        _fadeTween.MyCanvases = GetComponentsInChildren<CanvasRenderer>();
        _fadeTween.MyCanvasGroup = GetComponent<CanvasGroup>();
        SetUpTweeners();
    }

    private void SetUpTweeners()
    {
        SetUpPositionTween();
        SetUpRotationTween();
        SetUpPunchShakeTween();
        SetUpScaleTween();
        SetUpFadeTween();
    }

    private void SetUpPositionTween()
    {
        if (_positionTween != PositionTweenType.NoTween)
        {
            _counter++;
            _posTween.SetUpPositionTweens(_buildObjectsList, InEndEffect);
        }
    }

    private void SetUpRotationTween()
    {
        if (_rotationTween != RotationTweenType.NoTween)
        {
            _counter++;
            _rotateTween.SetUpRotateTweens(_buildObjectsList, StartCoroutines, InEndEffect);
        }
    }

    private void SetUpPunchShakeTween()
    {
        if (_punchShakeTween == PunchShakeTween.Punch)
        {
            if (_shakeOrPunchAtEnd == IsActive.No) _counter++;
            _punchTween.SetUpPunchTween(_buildObjectsList);
        }
        else if (_punchShakeTween == PunchShakeTween.Shake)
        {
            if (_shakeOrPunchAtEnd == IsActive.No) _counter++;
            _shakeTween.SetUpShakeTween(_buildObjectsList);
        }
    }

    private void SetUpScaleTween()
    {
        if (_scaleTransition != ScaleTween.NoTween)
        {
            _counter++;
            _scaleTween.SetUpScaleTweens(_buildObjectsList, StartCoroutines, InEndEffect);
        }
    }

    private void SetUpFadeTween()
    {
        if (_canvasGroupFade != FadeTween.NoTween)
        {
            _counter++;
            _fadeTween.SetUpFadeTweens(_canvasGroupFade);
        }
    }

    public void ActivateTweens(Action callBack)
    {
        if (_counter <= 0)
        {
            InTweenEndAction();
            return;
        }
        SetTweensUp(callBack, _globalInTime, TweenType.In, InTweenEndAction, IsActive.Yes);
    }

    public void DeactivateTweens(Action callBack)
    {
        if (_counter <= 0)
        {
            OutTweenEndAction();
            _endOfTweenAction.Invoke();
            return;
        }
        SetTweensUp(callBack, _globalOutTime, TweenType.Out, OutTweenEndAction, IsActive.No);
    }

    private void SetTweensUp(Action callBack, float globalTime, TweenType tweenType, 
                         TweenCallback callback, IsActive allowInTweenEffect)
    {
        StopAllCoroutines();
        _effectCounter = _counter;
        _finishedTweenCallback = callBack;
        _doEffectOnInTween = allowInTweenEffect;
        DoTweens(TimeToUseForTween(globalTime), tweenType, callback);
    }

    private void DoTweens(float tweenTime, TweenType tweenType, TweenCallback endaction)
    {
        _posTween.DoPositionTween(_positionTween, tweenTime, tweenType, endaction);
        _scaleTween.DoScaleTween(_scaleTransition, tweenTime, tweenType, endaction);
        _rotateTween.RotationTween(_rotationTween, tweenTime, tweenType, endaction);
        _fadeTween.DoCanvasFade(_canvasGroupFade, tweenTime, tweenType, endaction);
        
        if (_shakeOrPunchAtEnd == IsActive.No)
        {
            _punchTween.DoPunch(_punchShakeTween, tweenType, endaction);
            _shakeTween.DoShake(_punchShakeTween, tweenType, endaction);
        }
    }

    private void StartCoroutines(IEnumerator enumerator) // todo Replcae with corutine starter method
    {
        StartCoroutine(enumerator);
    }
    
    private void InTweenEndAction()
    {
        _effectCounter--;
        if (_effectCounter > 0) return;
        _middleOfTweenAction.Invoke();
        _finishedTweenCallback.Invoke();
    }

    private void OutTweenEndAction()
    {
        _effectCounter--;
        if (_effectCounter > 0) return;
        _endOfTweenAction.Invoke();
        _finishedTweenCallback.Invoke();
    }
    private float TimeToUseForTween(float timeToUse)
    {
        if (_useGlobalTime == IsActive.Yes)
        {
            return timeToUse;
        }
        return 0;
    }
    
    private void InEndEffect(RectTransform uiObject = null)
    {
        if (_shakeOrPunchAtEnd == IsActive.No) return;
        if (_punchShakeTween == PunchShakeTween.Shake)
        {
            _shakeTween.EndEffect(uiObject, _doEffectOnInTween);
        }
        if (_punchShakeTween == PunchShakeTween.Punch)
        {
            _punchTween.EndEffect(uiObject, _doEffectOnInTween);
        }
    }
}


