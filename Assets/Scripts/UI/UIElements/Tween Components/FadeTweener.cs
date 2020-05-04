using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

[System.Serializable]
public class FadeTweener
{
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] public float _InTime = 1;
    [SerializeField] [AllowNesting] [HideIf("UsingGlobalTime")] public float _OutTime = 1;
    [SerializeField] Ease _fadeEase = Ease.Unset;

    public bool UsingGlobalTime { get; set; }
    public CanvasGroup MyCanvasGroup { get; set; }
    Tweener _canvasInTweener;
    Tweener _canvasOutTweener;


    public void SetUpFadeInAndOut()
    {
        _canvasInTweener = MyCanvasGroup.DOFade(1, _InTime);
        _canvasOutTweener = MyCanvasGroup.DOFade(0, _OutTime);
        MyCanvasGroup.alpha = 0;
    }

    public void SetUpOutTween()
    {
        _canvasInTweener = MyCanvasGroup.DOFade(1, _InTime);
        _canvasOutTweener = MyCanvasGroup.DOFade(0, _OutTime);
        MyCanvasGroup.alpha = 1;
    }

    public void FadeIn(float time, bool activate, TweenCallback tweenCallback)
    {
        if (activate)
        {
            _canvasInTweener.Rewind();
            _canvasOutTweener.Pause();
            _canvasInTweener.ChangeStartValue(MyCanvasGroup.alpha, time).SetEase(_fadeEase).Play().OnComplete(tweenCallback);
        }
        else
        {
            tweenCallback.Invoke();
        }
    }

    public void FadeOut(float time, bool activate, TweenCallback tweenCallback)
    {
        if (activate)
        {
            tweenCallback.Invoke();
            _canvasOutTweener.Rewind();
        }
        else
        {
            _canvasInTweener.Pause();
            _canvasOutTweener.ChangeStartValue(MyCanvasGroup.alpha, time).SetEase(_fadeEase).Play().OnComplete(tweenCallback);
        }
    }
}
