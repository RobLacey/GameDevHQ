using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using NaughtyAttributes;

public class UITweener : MonoBehaviour
{
    [SerializeField] [ReorderableList] [Label("List Of Objects To Apply Effects To")] 
    List<BuildSettings> _applyEffectsTo = new List<BuildSettings>();

    [InfoBox("Add PARENT to apply effects as a whole or add each CHILD for a build effect. List is DRAG & DROP ordable", order = 0)]
    [Header("Effect Tween Settings", order = 1)] [HorizontalLine(4, color: EColor.Blue, order = 2)]
    [SerializeField] bool _useGlobalTweenTime = false;
    [SerializeField] [ShowIf("GlobalTime")] float _globalInTime = 1;
    [SerializeField] [ShowIf("GlobalTime")] float _globalOutTime = 1;
    [SerializeField] [Label("Settings")] [ShowIf("InPosition")] [BoxGroup("Position Tween")] PositionTween _posTween = new PositionTween();
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
            _fadeTween.UsingGlobalTime = true;
            _scaleTween.UsingGlobalTime = true;
            _posTween.UsingGlobalTime = true;
            return true;
        }
        else
        {
            _fadeTween.UsingGlobalTime = false;
            _scaleTween.UsingGlobalTime = false;
            _posTween.UsingGlobalTime = false;
            return false;
        }
    }

    public bool InPosition()
    {
        PositionInTween inPos = GetComponent<UIBranch>()._positionInTween;
        PositionOutTween outPos = GetComponent<UIBranch>()._positionOutTween;
        if (inPos != PositionInTween.NoTween || outPos != PositionOutTween.NoTween) { return true; }
        return false;
    }
    public bool Scale()
    {
        ScaleTween scale = GetComponent<UIBranch>()._scaleTransition;
        if (scale == ScaleTween.Scale_InAndOut || scale == ScaleTween.Scale_InOnly || scale == ScaleTween.Scale_OutOnly)
        { return true; }
        return false;
    }

    public bool Punch()
    {
        ScaleTween scale = GetComponent<UIBranch>()._scaleTransition;
        if (scale == ScaleTween.Punch) { return true; }
        return false;
    }

    public bool Shake()
    {
        ScaleTween scale = GetComponent<UIBranch>()._scaleTransition;
        if (scale == ScaleTween.Shake) { return true; }
        return false;
    }

    public bool Fade()
    {
        FadeTween fade = GetComponent<UIBranch>()._canvasGroupFade;
        if (fade != FadeTween.NoTween) { return true; }
        return false;
    }

    #endregion

    //Variables
    bool _positionInAndOut;
    bool _scaleInAndOut;

    private void Awake()
    {
        _fadeTween.MyCanvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetUpFadeTweens(FadeTween fadeTween)
    {
        if (fadeTween == FadeTween.FadeIn || fadeTween == FadeTween.FadeInAndOut)
        {
            _fadeTween.SetUpFadeInAndOut();
        }
        if (fadeTween == FadeTween.FadeOut)
        {
            _fadeTween.SetUpOutTween();
        }
    }

    public void SetUpPositionTweens(PositionOutTween positioOutTween, PositionInTween positionInTween)
    {
        if (positionInTween == PositionInTween.In)
        {
            _posTween.SetUpIn(_applyEffectsTo);
            _positionInAndOut = true;
        }
        else if (positioOutTween == PositionOutTween.Out)
        {
            _posTween.SetUpOut(_applyEffectsTo);
        }
    }

    public void SetUpScaleTweens(ScaleTween scaleTweenWType)
    {

        if (scaleTweenWType == ScaleTween.Scale_InOnly || scaleTweenWType == ScaleTween.Scale_InAndOut)
        {
            _scaleTween.SetUpInAndOutTween(_applyEffectsTo);
            _scaleInAndOut = true;
        }

        if (scaleTweenWType == ScaleTween.Scale_OutOnly)
        {
            _scaleTween.SetUpOutTween(_applyEffectsTo);
        }

        if (scaleTweenWType == ScaleTween.Punch)
        {
            _punchTween.SetUpPunchTween(_applyEffectsTo);
        }

        if (scaleTweenWType == ScaleTween.Shake)
        {
            _shakeTween.SetUpShakeTween(_applyEffectsTo);
        }
    }

    public void ScaleInTween(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            PauseAllTweens(_scaleTween._scaleOutTweeners);
            _scaleTween.RewindScaleTweens(_applyEffectsTo, _scaleTween._scaleInTweeners, _scaleTween._resetInOutscale);

            StartCoroutine(_scaleTween.ScaleSequence(_applyEffectsTo, _scaleTween._scaleInTweeners, 
                                         SetInTimeToUse(_scaleTween._InTime), 
                                         _scaleTween._easeIn, tweenCallback));
        }
        else
        {
            tweenCallback.Invoke();
        }
    }

    public void ScaleOutTween(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            _scaleTween.RewindScaleTweens(_applyEffectsTo, _scaleTween._scaleOutTweeners, _scaleTween._resetOutscale);
            tweenCallback.Invoke();
        }
        else
        {
            PauseAllTweens(_scaleTween._scaleInTweeners);

            if (_scaleInAndOut)
            {
                StartCoroutine(_scaleTween.ScaleSequence(_scaleTween._reversedBuildSettings, _scaleTween._scaleOutTweeners, 
                                             SetOutTimeToUse(_scaleTween._OutTime), 
                                             _scaleTween._easeOut, tweenCallback));
            }
            else
            {
                StartCoroutine(_scaleTween.ScaleSequence(_applyEffectsTo, _scaleTween._scaleOutTweeners, 
                                                         SetOutTimeToUse(_scaleTween._OutTime), _scaleTween._easeOut, 
                                                         tweenCallback));
            }
        }
    }

    public void DoInTween(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            PauseAllTweens(_posTween._outTweeners);
            _posTween.RewindPositionTweens(_applyEffectsTo, _posTween._inTweeners);
            StartCoroutine(_posTween.MoveSequence(_applyEffectsTo, _posTween._inTweeners, 
                                                        SetInTimeToUse(_posTween._inTime), 
                                                        _posTween._easeIn, tweenCallback));
        }
        else
        {
            tweenCallback.Invoke();
        }
    }

    public void DoOutTween(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            _posTween.RewindPositionTweens(_applyEffectsTo, _posTween._outTweeners);
            tweenCallback.Invoke();
        }
        else
        {
            PauseAllTweens(_posTween._inTweeners);

            if (_positionInAndOut)
            {
                StartCoroutine(_posTween.MoveSequence(_posTween._reversedBuild, _posTween._outTweeners,
                                                           SetOutTimeToUse(_posTween._outTime),
                                                           _posTween._easeOut, tweenCallback));

            }
            else
            {
                StartCoroutine(_posTween.MoveSequence(_applyEffectsTo, _posTween._outTweeners,
                                                            SetOutTimeToUse(_posTween._outTime),
                                                            _posTween._easeOut, tweenCallback));
            }
        }
    }

    public void DoCanvasFadeIn(bool activate, TweenCallback tweenCallback = null)
    {
        _fadeTween._canvasOutTweener.Pause();
        float time = SetInTimeToUse(_fadeTween._InTime);
        _fadeTween.FadeIn(time, activate, tweenCallback);
    }

    public void DoCanvasFadeOut(bool activate, TweenCallback tweenCallback = null)
    {
        _fadeTween._canvasInTweener.Pause();
        float time = SetInTimeToUse(_fadeTween._OutTime);
        _fadeTween.FadeOut(time, activate, tweenCallback);
    }

    public void DoPunch(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            if (_punchTween.CheckInEffectType)
            {
                _punchTween.RewindScaleTweens(_applyEffectsTo,_punchTween._punchTweeners);
                StartCoroutine(_punchTween.PunchSequence(_applyEffectsTo,_punchTween._punchTweeners, tweenCallback));
            }
            else
            {
                _punchTween.RewindScaleTweens(_applyEffectsTo, _punchTween._punchTweeners);
                tweenCallback.Invoke();
            }
        }
        else
        {
            if (_punchTween.CheckOutEffectType)
            {
                _punchTween.RewindScaleTweens(_applyEffectsTo, _punchTween._punchTweeners);
                StartCoroutine(_punchTween.PunchSequence(_applyEffectsTo,_punchTween._punchTweeners, tweenCallback));
            }
            else
            {
                tweenCallback.Invoke();
            }
        }
    }

    public void DoShake(bool activate, TweenCallback tweenCallback = null)
    {
        if (activate)
        {
            if (_shakeTween.CheckInEffectType)
            {
                _shakeTween.RewindScaleTweens(_applyEffectsTo,_shakeTween._shakeTweeners);
                StartCoroutine(_shakeTween.ShakeSequence(_applyEffectsTo,_shakeTween._shakeTweeners, tweenCallback));
            }
            else
            {
                _shakeTween.RewindScaleTweens(_applyEffectsTo, _shakeTween._shakeTweeners);
                tweenCallback.Invoke();
            }
        }
        else
        {
            if (_shakeTween.CheckOutEffectType)
            {
                _shakeTween.RewindScaleTweens(_applyEffectsTo, _shakeTween._shakeTweeners);
                StartCoroutine(_shakeTween.ShakeSequence(_applyEffectsTo,_shakeTween._shakeTweeners, tweenCallback));
            }
            else
            {
                tweenCallback.Invoke();
            }
        }
    }

    public void PauseAllTweens(List<Tweener> tweeners)
    {
        foreach (var item in tweeners)
        {
            item.Pause();
        }
    }

    private float SetOutTimeToUse(float tweenTime)
    {
        if (_useGlobalTweenTime)
        {
            return _globalOutTime;
        }
        else
        {
            return tweenTime;
        }
    }

    private float SetInTimeToUse(float tweenTime)
    {
        if (_useGlobalTweenTime)
        {
            return _globalInTime;
        }
        else
        {
            return tweenTime;
        }
    }
}

[Serializable]
public class BuildSettings
{
    [SerializeField] public RectTransform _element;
    [SerializeField] public Vector2 _tweenAnchorPosition;
    [SerializeField] public float _buildNextAfterDelay;
    [HideInInspector] public Vector2 _resetStartPositionStore;
}

