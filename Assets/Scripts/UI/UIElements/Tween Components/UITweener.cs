using System.Collections;
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
    [SerializeField] [DisableIf("IsRunning")] public PositionTweenType _positionTween = PositionTweenType.NoTween;
    [SerializeField] [DisableIf("IsRunning")] public RotationTweenType _rotationTween = RotationTweenType.NoTween;
    [SerializeField] [DisableIf("IsRunning")] public ScaleTween _scaleTransition = ScaleTween.NoTween;
    [SerializeField] [DisableIf("IsRunning")] public FadeTween _canvasGroupFade = FadeTween.NoTween;
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
    public bool Position() { return _positionTween != PositionTweenType.NoTween; }
    public bool Rotation() { return _rotationTween != RotationTweenType.NoTween; }
    public bool Scale() { return _scaleTransition != ScaleTween.Punch && _scaleTransition != ScaleTween.Shake 
                          && _scaleTransition != ScaleTween.NoTween; }
    public bool Punch()  { return _scaleTransition == ScaleTween.Punch; }
    public bool Shake() { return _scaleTransition == ScaleTween.Shake; }
    public bool Fade() {  return _canvasGroupFade != FadeTween.NoTween; }

    #endregion

    //Variables
    bool _positionInAndOut;
    bool _rotateInAndOut;
    bool _scaleInAndOut;
    int _counter = 0;
    int _endOfEffectCounter;
    int _startOfEffectCounter;
    public bool IsRunning { get; set; } = false;              //To disable Tween settings as they break when changed during runtime

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

    //Expand for Detail
    private void SetUpTweeners()
    {
        if (_positionTween != PositionTweenType.NoTween)
        {
            _counter++;
            _posTween.SetUpPositionTweens(_positionTween, _buildObjectsList);
            if (_positionTween == PositionTweenType.InAndOut) _positionInAndOut = true;
        }

        if (_rotationTween != RotationTweenType.NoTween)
        {
            _counter++;
            _rotateTween.SetUpRotateTweens(_rotationTween, _buildObjectsList);
            if (_rotationTween == RotationTweenType.InAndOut) _rotateInAndOut = true;
        }

        if (_scaleTransition != ScaleTween.NoTween)
        {
            if (_scaleTransition == ScaleTween.Punch)
            {
                _counter++;
                _punchTween.SetUpPunchTween(_buildObjectsList);
            }

            else if (_scaleTransition == ScaleTween.Shake)
            {
                _counter++;
                _shakeTween.SetUpShakeTween(_buildObjectsList);
            }
            else
            {
                _counter++;
                _scaleTween.SetUpScaleTweens(_scaleTransition, _buildObjectsList);
                if(_scaleTransition == ScaleTween.Scale_InAndOut) _scaleInAndOut = true;
            }
        }

        if (_canvasGroupFade != FadeTween.NoTween)
        {
            _counter++;
            _fadeTween.SetUpFadeTweens(_canvasGroupFade);
        }
    }

    //Expand for Detail
    public void ActivateTweens(Action callBack)
    {
        StopAllCoroutines();
        _endOfEffectCounter = _counter;
        _startOfEffectCounter = _counter;
        _InTweensCallback = callBack;

        if (_startOfEffectCounter == 0)
        {
            InTweenEndAction();
        }

        if (_positionTween == PositionTweenType.In || _positionTween == PositionTweenType.InAndOut)
        {
            DoInTween(true, () => InTweenEndAction());
        }
        else if (_positionTween == PositionTweenType.Out)
        {
            DoOutTween(true, () => InTweenEndAction());
        }

        if (_rotationTween == RotationTweenType.In || _rotationTween == RotationTweenType.InAndOut)
        {
            DoRotateInTween(true, () => InTweenEndAction());
        }
        else if (_rotationTween == RotationTweenType.Out)
        {
            DoRotateOutTween(true, () => InTweenEndAction());
        }

        if (_scaleTransition == ScaleTween.Scale_InOnly || _scaleTransition == ScaleTween.Scale_InAndOut)
        {
            ScaleInTween(true, () => InTweenEndAction());
        }
        else if (_scaleTransition == ScaleTween.Scale_OutOnly)
        {
            ScaleOutTween(true, () => InTweenEndAction());
        }
        else if (_scaleTransition == ScaleTween.Punch)
        {
            DoPunch(true, () => InTweenEndAction());
        }
        else if (_scaleTransition == ScaleTween.Shake)
        {
            DoShake(true, () => InTweenEndAction());
        }

        if (_canvasGroupFade == FadeTween.FadeIn || _canvasGroupFade == FadeTween.FadeInAndOut)
        {
            DoCanvasFadeIn(true, () => InTweenEndAction());
        }
        else if (_canvasGroupFade == FadeTween.FadeOut)
        {
            DoCanvasFadeOut(true, () => InTweenEndAction());
        }
    }

    //Expand for Detail
    public void StartOutTweens(Action callBack)
    {

        _OutTweensCallback = callBack;

        if (_endOfEffectCounter == 0)
        {
            OutTweenEndAction();
            return;
        }

        if (_scaleTransition == ScaleTween.Scale_OutOnly || _scaleTransition == ScaleTween.Scale_InAndOut)
        {
            ScaleOutTween(false, () => OutTweenEndAction());
        }
        else if (_scaleTransition == ScaleTween.Scale_InOnly)
        {
            ScaleInTween(false, () => OutTweenEndAction());
        }
        else if (_scaleTransition == ScaleTween.Punch)
        {
            DoPunch(false, () => OutTweenEndAction());
        }
        else if (_scaleTransition == ScaleTween.Shake)
        {

            DoShake(false, () => OutTweenEndAction());
        }

        if (_positionTween == PositionTweenType.Out || _positionTween == PositionTweenType.InAndOut)
        {
            DoOutTween(false, () => OutTweenEndAction());
        }
        else if (_positionTween == PositionTweenType.In)
        {
            DoInTween(false, () => OutTweenEndAction());
        }

        if (_rotationTween == RotationTweenType.Out || _rotationTween == RotationTweenType.InAndOut)
        {
            DoRotateOutTween(false, () => OutTweenEndAction());
        }
        else if (_rotationTween == RotationTweenType.In)
        {
            DoRotateInTween(false, () => OutTweenEndAction());
        }

        if (_canvasGroupFade == FadeTween.FadeOut || _canvasGroupFade == FadeTween.FadeInAndOut)
        {
            DoCanvasFadeOut(false, () => OutTweenEndAction());
        }
        else if (_canvasGroupFade == FadeTween.FadeIn)
        {
            DoCanvasFadeIn(false, () => OutTweenEndAction());
        }

        _endOfTweenAction.Invoke(false);

    }

    //Scale Tween
    #region ScaleTween

    private void ScaleInTween(bool isIn, TweenCallback tweenCallback = null)
    {
        if (isIn)
        {
            _scaleTween.PauseOutTweens();
            _scaleTween.RewindScaleInTweens();
            _scaleTween.InSettings(SetInTimeToUse());
            StartCoroutine(_scaleTween.ScaleSequence(tweenCallback));
        }
        else
        {
            tweenCallback.Invoke();
        }
    }

    private void ScaleOutTween(bool isOut, TweenCallback tweenCallback = null)
    {
        if (isOut)
        {
            _scaleTween.RewindScaleOutTweens();
            tweenCallback.Invoke();
        }
        else
        {
            _scaleTween.PauseInTweens();

            if (_scaleInAndOut)
            {
                _scaleTween.InOutSettings(SetOutTimeToUse());
                StartCoroutine(_scaleTween.ScaleSequence(tweenCallback));
            }
            else
            {
                _scaleTween.OutSettings(SetOutTimeToUse());
                StartCoroutine(_scaleTween.ScaleSequence(tweenCallback));
            }
        }
    } 
    #endregion

    //Position Tween
    #region PositionTween

    private void DoInTween(bool isIn, TweenCallback tweenCallback = null)
    {
        if (isIn)
        {
            _posTween.PauseOutTweens();

            if (!_positionInAndOut) 
            { 
                _posTween.RewindInTweens(); 
            }
            _posTween.InSettings(SetInTimeToUse());
            StartCoroutine(_posTween.MoveSequence(tweenCallback));
        }
        else
        {
            tweenCallback.Invoke();
        }
    }

    private void DoOutTween(bool isOut, TweenCallback tweenCallback = null)
    {
        if (isOut)
        {
            _posTween.RewindOutTweens();
            tweenCallback.Invoke();
        }
        else
        {
            _posTween.PauseInTweens();

            if (_positionInAndOut)
            {
                _posTween.InOutSettings(SetOutTimeToUse());
                StartCoroutine(_posTween.MoveSequence(tweenCallback));

            }
            else
            {
                _posTween.OutSettings(SetOutTimeToUse());
                StartCoroutine(_posTween.MoveSequence(tweenCallback));
            }
        }
    }
    #endregion

    //Rotation Tweens
    #region Rotation Tweens
    private void DoRotateInTween(bool isIn, TweenCallback tweenCallback = null)
    {
        if (isIn)
        {
            if (!_rotateInAndOut)
            {
                _rotateTween.RewindRotateTweens();

            }
            _rotateTween.KIllRunningTweens();
            _rotateTween.InSettings(SetInTimeToUse());
            StartCoroutine(_rotateTween.MoveSequence(RotationTweenType.In, tweenCallback));
        }
        else
        {
            tweenCallback.Invoke();
        }
    }

    private void DoRotateOutTween(bool isOut, TweenCallback tweenCallback = null)
    {
        if (isOut)
        {
            _rotateTween.RewindRotateTweens();
            _rotateTween.KIllRunningTweens();
            tweenCallback.Invoke();
        }
        else
        {
            if (_rotateInAndOut)
            {
                _rotateTween.InOutSettings(SetOutTimeToUse());
                StartCoroutine(_rotateTween.MoveSequence(RotationTweenType.InAndOut, tweenCallback));
            }
            else
            {
                _rotateTween.OutSettings(SetOutTimeToUse());
                StartCoroutine(_rotateTween.MoveSequence(RotationTweenType.Out, tweenCallback));
            }
        }
    }
    #endregion

    //Fade Tweens
    #region Fade Tweens
    private void DoCanvasFadeIn(bool isIn, TweenCallback tweenCallback = null)
    {
        _fadeTween._canvasOutTweener.Pause();
        _fadeTween.InSettings(SetInTimeToUse());
        _fadeTween.FadeIn(isIn, tweenCallback);
    }

    private void DoCanvasFadeOut(bool isOut, TweenCallback tweenCallback = null)
    {
        _fadeTween._canvasInTweener.Pause();
        _fadeTween.OutSettings(SetOutTimeToUse());
        _fadeTween.FadeOut(isOut, tweenCallback);
    }
    #endregion

    //Punch & Shake Tweens
    #region Punch & Shake Tweens
    private void DoPunch(bool isIn, TweenCallback tweenCallback = null)
    {
        if (isIn)
        {
            if (_punchTween.CheckInEffectType())
            {
                _punchTween.RewindTweens();
                StartCoroutine(_punchTween.PunchSequence(tweenCallback));
            }
            else
            {
                _punchTween.RewindTweens();
                tweenCallback.Invoke();
            }
        }
        else
        {
            if (_punchTween.CheckOutEffectType())
            {
                _punchTween.RewindTweens();
                StartCoroutine(_punchTween.PunchSequence(tweenCallback));
            }
            else
            {
                tweenCallback.Invoke();
            }
        }
    }

    private void DoShake(bool isOut, TweenCallback tweenCallback = null)
    {
        if (isOut)
        {
            if (_shakeTween.CheckInEffectType())
            {
                _shakeTween.RewindScaleTweens();
                StartCoroutine(_shakeTween.ShakeSequence(tweenCallback));
            }
            else
            {
                _shakeTween.RewindScaleTweens();
                tweenCallback.Invoke();
            }
        }
        else
        {
            if (_shakeTween.CheckOutEffectType())
            {
                _shakeTween.RewindScaleTweens();
                StartCoroutine(_shakeTween.ShakeSequence(tweenCallback));
            }
            else
            {
                tweenCallback.Invoke();
            }
        }
    }
    #endregion

    private void OutTweenEndAction()
    {
        _endOfEffectCounter--;
        if (_endOfEffectCounter <= 0)
        {
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


