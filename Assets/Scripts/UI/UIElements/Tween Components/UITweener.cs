﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using NaughtyAttributes;
using UnityEngine.Events;

public class UITweener : MonoBehaviour
{
    [SerializeField] [ReorderableList] [Label("List Of Objects To Apply Effects To")] 
    List<BuildSettings> _buildObjectsList = new List<BuildSettings>();

    [InfoBox("Add PARENT to apply effects as a whole or add each CHILD for a build effect. List is DRAG & DROP orderable", order = 0)]
    [Header("Effect Tween Settings", order = 1)] [HorizontalLine(4, color: EColor.Blue, order = 2)]
    [SerializeField] bool _useGlobalTweenTime = false;
    [SerializeField] [ShowIf("GlobalTime")] float _globalInTime = 1;
    [SerializeField] [ShowIf("GlobalTime")] float _globalOutTime = 1;
    [Header("Tween Settings")]
    [SerializeField] public PositionTweenType _positionTween = PositionTweenType.NoTween;
    [SerializeField] public RotationTweenType _rotationTween = RotationTweenType.NoTween;
    [SerializeField] public ScaleTween _scaleTransition = ScaleTween.NoTween;
    [SerializeField] public FadeTween _canvasGroupFade = FadeTween.NoTween;
    [Header("Tween Trigger Settings")]
    [SerializeField] [Label("Event At End/Mid-Point of Tween")] TweenTrigger _endOfTweenAction;
    [SerializeField] [Label("Settings")] [ShowIf("Position")] [BoxGroup("Position Tween")] PositionTween _posTween = new PositionTween();
    [SerializeField] [Label("Settings")] [ShowIf("Rotation")] [BoxGroup("Rotation Tween")] RotateTween _rotateTween = new RotateTween();
    [SerializeField] [Label("Settings")] [ShowIf("Scale")] [BoxGroup("Scale Tween")] ScaleTweener _scaleTween = new ScaleTweener();
    [SerializeField] [Label("Settings")] [ShowIf("Punch")] [BoxGroup("Punch Tween")] PunchTweener _punchTween = new PunchTweener();
    [SerializeField] [Label("Settings")] [ShowIf("Shake")] [BoxGroup("Shake Tween")] ShakeTweener _shakeTween = new ShakeTweener();
    [SerializeField] [Label("Settings")] [ShowIf("Fade")] [BoxGroup("Fade Tween")] FadeTweener _fadeTween = new FadeTweener();

    //EditorScripts
    #region Expand Here

    public bool GlobalTime() 
    {
        if (_useGlobalTweenTime)
        {
            _posTween.UsingGlobalTime = true;
            _scaleTween.UsingGlobalTime = true;
            _fadeTween.UsingGlobalTime = true;
            _rotateTween.UsingGlobalTime = true;
        }
        else
        {
            _posTween.UsingGlobalTime = false;
            _scaleTween.UsingGlobalTime = false;
            _fadeTween.UsingGlobalTime = false;
            _rotateTween.UsingGlobalTime = false;

        }
        return _useGlobalTweenTime; 
    }
    public bool Position() 
    {
        foreach (var item in _buildObjectsList)
        {
            item.PositionTween = _positionTween != PositionTweenType.NoTween;
        }
        return _positionTween != PositionTweenType.NoTween; 
    }
    public bool Rotation() 
    {
        foreach (var item in _buildObjectsList)
        {
            item.RotationTween = _rotationTween != RotationTweenType.NoTween;
        }
        return _rotationTween != RotationTweenType.NoTween; 
    }
    public bool Scale() 
    {
        bool active = _scaleTransition != ScaleTween.Punch && _scaleTransition != ScaleTween.Shake
                          && _scaleTransition != ScaleTween.NoTween;
        foreach (var item in _buildObjectsList)
        {
            item.ScaleTween = active;
        }
        return active; 
    }
    public bool Punch()  { return _scaleTransition == ScaleTween.Punch; }
    public bool Shake() { return _scaleTransition == ScaleTween.Shake; }
    public bool Fade() {  return _canvasGroupFade != FadeTween.NoTween; }

    #endregion

    //Variables
    int _counter = 0;
    int _endOfEffectCounter;
    int _startOfEffectCounter;
    public bool IsRunning { get; set; } = false;              //Use To disable Tween settings as they break when changed during runtime

    Action _InTweensCallback;
    Action _OutTweensCallback;

    //Classes
    [Serializable]
    public class TweenTrigger : UnityEvent<bool> { }

    public void OnAwake(CanvasGroup canvasGroup)
    {
        _fadeTween.MyCanvasGroup = canvasGroup;
        SetUpTweeners();
    }

    private void SetUpTweeners()
    {
        if (_positionTween != PositionTweenType.NoTween)
        {
            _counter++;
            _posTween.SetUpPositionTweens( _buildObjectsList, (x) => StartCoroutines(x));
        }

        if (_rotationTween != RotationTweenType.NoTween)
        {
            _counter++;
            _rotateTween.SetUpRotateTweens(_buildObjectsList, (x) => StartCoroutines(x));
        }

        if (_scaleTransition != ScaleTween.NoTween)
        {
            if (_scaleTransition == ScaleTween.Punch)
            {
                _counter++;
                _punchTween.SetUpPunchTween(_buildObjectsList, (x) => StartCoroutines(x));
            }

            else if (_scaleTransition == ScaleTween.Shake)
            {
                _counter++;
                _shakeTween.SetUpShakeTween(_buildObjectsList, (x) => StartCoroutines(x));
            }
            else
            {
                _counter++;
                _scaleTween.SetUpScaleTweens(_buildObjectsList, (x) => StartCoroutines(x));
            }

        }
        if (_canvasGroupFade != FadeTween.NoTween)
        {
            _counter++;
            _fadeTween.SetUpFadeTweens(_canvasGroupFade);
        }
    }

    public void ActivateTweens(Action callBack)
    {
        _endOfEffectCounter = _counter;
        _startOfEffectCounter = _counter;
        _InTweensCallback = callBack;

        if (_startOfEffectCounter == 0)
        {
            InTweenEndAction();
        }

         _posTween.DoPositionTween(_positionTween, SetInTimeToUse(), true, () => InTweenEndAction());
        _scaleTween.ScaleTween(_scaleTransition, SetInTimeToUse(), true, () => InTweenEndAction());
        _rotateTween.RotationTween(_rotationTween, SetInTimeToUse(), true, () => InTweenEndAction());
        _fadeTween.DoCanvasFade(_canvasGroupFade, SetOutTimeToUse(), true, () => InTweenEndAction());
        _punchTween.DoPunch(_scaleTransition, true, () => InTweenEndAction());
        _shakeTween.DoShake(_scaleTransition, true, () => InTweenEndAction());
    }

    public void StartOutTweens(Action callBack)
    { 
        _OutTweensCallback = callBack;

        if (_endOfEffectCounter == 0)
        {
            OutTweenEndAction();
            _endOfTweenAction.Invoke(false);
            return;
        }

        _posTween.DoPositionTween(_positionTween, SetOutTimeToUse(), false, () => OutTweenEndAction());
        _scaleTween.ScaleTween(_scaleTransition, SetOutTimeToUse(), false, () => OutTweenEndAction());
        _rotateTween.RotationTween(_rotationTween, SetOutTimeToUse(), false, () => OutTweenEndAction());
        _fadeTween.DoCanvasFade(_canvasGroupFade, SetOutTimeToUse(), false, () => OutTweenEndAction());
        _punchTween.DoPunch(_scaleTransition, false, () => OutTweenEndAction());
        _shakeTween.DoShake(_scaleTransition, false, () => OutTweenEndAction());
    }

    private void StartCoroutines(IEnumerator enumerator)
    {
        StopAllCoroutines();
        StartCoroutine(enumerator);
    }

    private void OutTweenEndAction()
    {
        _endOfEffectCounter--;
        if (_endOfEffectCounter <= 0)
        {
            _endOfTweenAction.Invoke(false);
            _OutTweensCallback.Invoke();
        }
    }

    private void InTweenEndAction()
    {
        _startOfEffectCounter--;

        if (_startOfEffectCounter <= 0)
        {
            _endOfTweenAction.Invoke(true);
            _InTweensCallback.Invoke();
        }
    }

    private float SetOutTimeToUse()
    {
        if (_useGlobalTweenTime)
        {
            return _globalOutTime;
        }
        return 0;
    }

    private float SetInTimeToUse()
    {
        if (_useGlobalTweenTime)
        {
            return _globalInTime;
        }
        return 0;
    }
}


