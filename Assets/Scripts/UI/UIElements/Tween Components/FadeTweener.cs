using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using System;

[System.Serializable]
public class FadeTweener
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] Ease _fadeEase = Ease.Linear;

    //Variables
    float _startAlpha;
    float _tweenTime;
    public Tweener _canvasInTweener;
    public Tweener _canvasOutTweener;
    int _id;

    //Delegates & Properties
    public CanvasGroup MyCanvasGroup { get; set; }
    public bool UsingGlobalTime { get; set; }

    public void SetUpFadeTweens(FadeTween fadeTween)
    {
        if (fadeTween == FadeTween.FadeIn || fadeTween == FadeTween.FadeInAndOut)
        {
            MyCanvasGroup.alpha = 0;
            _startAlpha = 0;
        }
        if (fadeTween == FadeTween.FadeOut)
        {
            MyCanvasGroup.alpha = 1;
            _startAlpha = 1;
        }
    }

    public void DoCanvasFade(FadeTween fadeTween, float globalTime, bool isIn, TweenCallback tweenCallback = null)
    {
        if (fadeTween == FadeTween.NoTween) return;

        StopRunningTweens();

        if (fadeTween == FadeTween.FadeIn)
        {
            if (isIn)
            {
                MyCanvasGroup.alpha = _startAlpha;
                SetInTime(globalTime);
                Tween(1, tweenCallback);
            }
            else
            {
                tweenCallback.Invoke();
            }

        }
        if (fadeTween == FadeTween.FadeOut)
        {
            if (isIn)
            {
                MyCanvasGroup.alpha = _startAlpha;
                tweenCallback.Invoke();
            }
            else
            {
                SetOutTime(globalTime);
                Tween(0, tweenCallback);
            }
        }
        if (fadeTween == FadeTween.FadeInAndOut)
        {
            if (isIn)
            {
                SetInTime(globalTime);
                Tween(1, tweenCallback);
            }
            else
            {
                SetOutTime(globalTime);
                Tween(0, tweenCallback);
            }

        }
    }

    private void StopRunningTweens()
    {
        DOTween.Kill("fade" + MyCanvasGroup.GetInstanceID());
    }

    private void Tween(float targetAlpha, TweenCallback tweenCallback)
    {
        MyCanvasGroup.DOFade(targetAlpha, _tweenTime)
                                .SetId("fade" + MyCanvasGroup.GetInstanceID())
                                .SetEase(_fadeEase).SetAutoKill(true)
                                .Play()
                                .OnComplete(tweenCallback);

    }

    private void SetInTime(float globalTime) 
    {
        if (globalTime > 0)
        {
            _tweenTime = globalTime;
        }
        else
        {
            _tweenTime = _inTime;
        }
    }

    private void SetOutTime(float globalTime) 
    {
        if (globalTime > 0)
        {
            _tweenTime = globalTime;
        }
        else
        {
            _tweenTime = _outTime;
        }
    }

}
