using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

[System.Serializable]
public class FadeTweener
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _inTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] float _outTime = 1;
    [SerializeField] Ease _fadeEase = Ease.Unset;

    //Variables
    float _tweenTime;
    public Tweener _canvasInTweener;
    public Tweener _canvasOutTweener;

    //Delegates & Properties
    public CanvasGroup MyCanvasGroup { get; set; }
    public bool UsingGlobalTime { get; set; }


    public void SetUpFadeTweens(FadeTween fadeTween)
    {
        if (fadeTween == FadeTween.FadeIn || fadeTween == FadeTween.FadeInAndOut)
        {
            SetUpFadeInAndOut();
        }
        if (fadeTween == FadeTween.FadeOut)
        {
            SetUpOutTween();
        }
    }

    public void SetUpFadeInAndOut()
    {
        _canvasInTweener = MyCanvasGroup.DOFade(1, _inTime);
        _canvasOutTweener = MyCanvasGroup.DOFade(0, _outTime);
        MyCanvasGroup.alpha = 0;
    }

    public void SetUpOutTween()
    {
        _canvasInTweener = MyCanvasGroup.DOFade(1, _inTime);
        _canvasOutTweener = MyCanvasGroup.DOFade(0, _outTime);
        MyCanvasGroup.alpha = 1;
    }

    public void FadeIn(bool activate, TweenCallback tweenCallback)
    {
        if (activate)
        {
            _canvasInTweener.Rewind();
            _canvasOutTweener.Pause();
            _canvasInTweener.ChangeStartValue(MyCanvasGroup.alpha, _tweenTime).SetEase(_fadeEase).Play().OnComplete(tweenCallback);
        }
        else
        {
            tweenCallback.Invoke();
        }
    }

    public void FadeOut(bool activate, TweenCallback tweenCallback)
    {
        if (activate)
        {
            tweenCallback.Invoke();
            _canvasOutTweener.Rewind();
        }
        else
        {
            _canvasInTweener.Pause();
            _canvasOutTweener.ChangeStartValue(MyCanvasGroup.alpha, _tweenTime).SetEase(_fadeEase).Play().OnComplete(tweenCallback);
        }
    }

    public void InSettings(float globalTime)
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
    public void OutSettings(float globalTime)
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
