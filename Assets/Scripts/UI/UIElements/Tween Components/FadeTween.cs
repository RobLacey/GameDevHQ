using DG.Tweening;
using System;

[Serializable]
public class FadeTween : TweenBase
{
    private float _targetAlpha;
    private bool _firstTime = true;

   
    protected override Tween DoTweenProcess(BuildTweenData item, TweenCallback callback)
    {
        return item.MyCanvasGroup.DOFade(_targetAlpha, _tweenTime)
                            .SetId($"{_tweenName}{item.Element.GetInstanceID()}")
                            .SetEase(_tweenEase)
                            .SetAutoKill(true)
                            .Play()
                            .OnComplete(callback);
    }

    protected override void RewindTweens()
    {
        if (_tweenType == TweenStyle.In || _tweenType == TweenStyle.InAndOut)
        {
            SetUpCanvasGroup(alphaPreset:0);
            //MyCanvasGroup.alpha = 0;
        }
        else
        {
            SetUpCanvasGroup(alphaPreset:1);
            //MyCanvasGroup.alpha = 1;
        }
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenType == TweenStyle.In || _tweenType == TweenStyle.InAndOut)
        {
            SetUpCanvasGroup(alphaPreset: 0);
            _targetAlpha = 1;
        }
        else
        {
            SetUpCanvasGroup(alphaPreset: 1);
            _targetAlpha = 0;
        }
        _firstTime = false;
    }

    private void SetUpCanvasGroup(float alphaPreset)
    {
        foreach (var item in _listToUse)
        {
            if (_firstTime)
                item.MyCanvasGroup.alpha = alphaPreset;
        }
    }

    protected override void OutTweenTargetSettings()
    {
        _targetAlpha = 0;
    }
}
