using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections;

// ReSharper disable IdentifierTypo
[Serializable]
public class FadeTweener
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] Ease _fadeEase = Ease.Linear;

    //Variables
    private float _startAlpha;
    private FadeTween _tweenType;
    private float _tweenTime;
    private float _globalTime;
    private TweenCallback _callback;
    protected Coroutine _coroutine;

    public CanvasGroup MyCanvasGroup { get; set; }
    public bool UsingGlobalTime { get; set; }

    public void SetUpTweens(FadeTween fadeTween, CanvasGroup canvasGroup)
    {
        MyCanvasGroup = canvasGroup;
        
        if (fadeTween == FadeTween.FadeIn || fadeTween == FadeTween.FadeInAndOut)
        {
            MyCanvasGroup.alpha = 0;
            _startAlpha = 0;
        }
        else
        {
            MyCanvasGroup.alpha = 1;
            _startAlpha = 1;
        }
    }

    public void StartTween(Enum tweenType, float tweenTime, TweenType isIn, TweenCallback tweenCallback)
    {
        _tweenType = (FadeTween) tweenType;
        if (_tweenType == FadeTween.NoTween) return;
        StopRunningTweens();
        StaticCoroutine.StopCoroutines(_coroutine);
        _callback = tweenCallback;
        _globalTime = tweenTime;
        
        switch (_tweenType)
        {
            case FadeTween.FadeIn:
                InTween(isIn);
                break;
            case FadeTween.FadeOut:
                OutTween(isIn);
                break;
            case FadeTween.FadeInAndOut:
                InAndOutTween(isIn);
                break;
        }
    }

    private void InTween(TweenType isIn)
    {
        if (isIn == TweenType.In)
        {
            DoInTween();
        }
        else
        {
            RewindTweens();
            _callback.Invoke();
        }
    }
    
    private void OutTween(TweenType isIn)
    {
        if (isIn == TweenType.In)
        {
            RewindTweens();
            _callback.Invoke();
        }
        else
        {
            DoOutTween();
        }
    }
    
    private void InAndOutTween(TweenType isIn)
    {
        if (isIn == TweenType.In)
        {
            DoInTween();
        }
        else
        {
            DoOutTween();
        }
    }

    private void DoInTween()
    {
        SetInTime();
        _coroutine = StaticCoroutine.StartCoroutine(TweenSequence(1));
    }

    private void DoOutTween()
    {
        SetOutTime();
        _coroutine = StaticCoroutine.StartCoroutine(TweenSequence(0));
    }
    
    private IEnumerator TweenSequence(float targetAlpha)
    {
        MyCanvasGroup.DOFade(targetAlpha, _tweenTime)
                     .SetId($"fade{MyCanvasGroup.GetInstanceID()}")
                     .SetEase(_fadeEase).SetAutoKill(true)
                     .Play()
                     .OnComplete(_callback);
        yield return null;
    }

    private void RewindTweens()
    {
        MyCanvasGroup.alpha = _startAlpha;
    }

    private void StopRunningTweens()
    {
        DOTween.Kill($"fade{MyCanvasGroup.GetInstanceID()}");
    }

    private void SetInTime() => _tweenTime = _globalTime > 0 ? _globalTime : _inTime;
    
    private void SetOutTime() => _tweenTime = _globalTime > 0 ? _globalTime : _outTime;
}
