using System.Collections;
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
    [SerializeField] [Label("Shake/Punch As End Effect")] IsActive _shakeOrPunchAtEnd = IsActive.No;
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
    private int _endOfEffectCounter;
    private int _startOfEffectCounter;
    private bool _effectOnInTween;

    //Delegates
    private Action _inTweensCallback;
    private Action _outTweensCallback;
    private Action _resetTweens;

    //Classes
    [Serializable]
    public class TweenTrigger : UnityEvent<bool> { }

    public void OnAwake() 
    {
        _fadeTween.MyCanvases = GetComponentsInChildren<CanvasRenderer>();
        _fadeTween.MyCanvasGroup = GetComponent<CanvasGroup>();
        SetUpTweeners();
    }

    private void SetUpTweeners()
    {
        if (_positionTween != PositionTweenType.NoTween)
        {
            _counter++;
            _resetTweens += _posTween.SetUpPositionTweens( _buildObjectsList, StartCoroutines, EndEffectProcess);
        }

        if (_rotationTween != RotationTweenType.NoTween)
        {
            _counter++;
            _resetTweens += _rotateTween.SetUpRotateTweens(_buildObjectsList, StartCoroutines, EndEffectProcess);
        }

        SetUpPunchShakeTween();

        if (_scaleTransition != ScaleTween.NoTween)
        {
            _counter++;
            _resetTweens += _scaleTween.SetUpScaleTweens(_buildObjectsList, StartCoroutines, EndEffectProcess);
        }

        if (_canvasGroupFade != FadeTween.NoTween)
        {
            _counter++;
            _resetTweens += _fadeTween.SetUpFadeTweens(_canvasGroupFade);
        }
    }

    private void SetUpPunchShakeTween()
    {
        if (_punchShakeTween != PunchShakeTween.NoTween)
        {
            if (_punchShakeTween == PunchShakeTween.Punch)
            {
                if (_shakeOrPunchAtEnd == IsActive.No) _counter++;
                _punchTween.SetUpPunchTween(_buildObjectsList, StartCoroutines);
            }
            else if (_punchShakeTween == PunchShakeTween.Shake)
            {
                if (_shakeOrPunchAtEnd == IsActive.No) _counter++;
                _shakeTween.SetUpShakeTween(_buildObjectsList, StartCoroutines);
            }
        }
    }

    public void ActivateTweens(Action callBack)
    {
        StopAllCoroutines();
        _startOfEffectCounter = _counter;
        _inTweensCallback = callBack;

        if (_startOfEffectCounter <= 0)
        {
            InTweenEndAction();
        }

        _effectOnInTween = true; // Review as obscure
        float inTimeToUse = UseGlobalTime(_globalInTime);
        TriggerTweens(inTimeToUse, TweenType.In, InTweenEndAction);
    }

    public void DeactivateTweens(Action callBack)
    {
        StopAllCoroutines();
        _endOfEffectCounter = _counter;
        _outTweensCallback = callBack;

        if (_endOfEffectCounter <= 0)
        {
            OutTweenEndAction();
            _endOfTweenAction.Invoke(false);
            return;
        }

        _effectOnInTween = false; // Review as obscure
        float outTimeToUse = UseGlobalTime(_globalOutTime);
        TriggerTweens(outTimeToUse, TweenType.Out, OutTweenEndAction);
    }

    private void TriggerTweens(float inTimeToUse, TweenType tweenType, TweenCallback endaction)
    {
        _posTween.DoPositionTween(_positionTween, inTimeToUse, tweenType, endaction);
        _scaleTween.DoScaleTween(_scaleTransition, inTimeToUse, tweenType, endaction);
        _rotateTween.RotationTween(_rotationTween, inTimeToUse, tweenType, endaction);
        _fadeTween.DoCanvasFade(_canvasGroupFade, inTimeToUse, tweenType, endaction);
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
        _startOfEffectCounter--;

        if (_startOfEffectCounter <= 0)
        {
            _middleOfTweenAction.Invoke(true);
            _inTweensCallback.Invoke();
        }
    }

    private void OutTweenEndAction()
    {
        _endOfEffectCounter--;

        if (_endOfEffectCounter <= 0)
        {
            _endOfTweenAction.Invoke(false);
            _outTweensCallback.Invoke();
        }
    }
    private float UseGlobalTime(float timeToUse)
    {
        if (_useGlobalTime == IsActive.Yes)
        {
            return timeToUse;
        }
        return 0;
    }
    
    private void EndEffectProcess(RectTransform uiObject)
    {
        if (_shakeOrPunchAtEnd == IsActive.Yes)
        {
            if (_punchShakeTween == PunchShakeTween.Shake)
            {
                _shakeTween.EndEffect(uiObject, _effectOnInTween);
            }
            if (_punchShakeTween == PunchShakeTween.Punch)
            {
                _punchTween.EndEffect(uiObject, _effectOnInTween);
            }
        }
    }

    public void Reset()
    {
        _resetTweens?.Invoke();
    }
}


