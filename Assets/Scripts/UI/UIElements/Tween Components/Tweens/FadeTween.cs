using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IFadeTween: ITweenBase { }

[Serializable]
public class FadeTween : TweenBase, IFadeTween
{
    private float _targetAlpha;

    public override void SetUpTweens(List<BuildTweenData> buildObjectsList, 
                                     TweenScheme tweenScheme, 
                                     Action<BuildTweenData> effectCall)
    {
        base.SetUpTweens(buildObjectsList, tweenScheme, effectCall);

        foreach (var item in _buildList)
        {
            item.MyCanvasGroup.alpha = 0;
        }
    }

    public override void StartTween(TweenType tweenType, TweenCallback tweenCallback)
    {
        if (_scheme is null) return;
        _tweenStyle = _scheme.FadeTween;
        base.StartTween(tweenType, tweenCallback);
    }

    protected override void RewindTweens()
    {
        if (_tweenStyle == TweenStyle.In || _tweenStyle == TweenStyle.InAndOut)
        {
            SetUpCanvasGroup(alphaPreset:0);
        }
        else
        {
            SetUpCanvasGroup(alphaPreset:1);
        }
    }

    private void SetUpCanvasGroup(float alphaPreset)
    {
        foreach (var item in _buildList)
        {
            item.MyCanvasGroup.alpha = alphaPreset;
        }
    }

    protected override void DoInTween()
    {
        _tweenEase = _scheme.FadeData.EaseIn;
        _tweenTime = _scheme.SetFadeTime(TweenType.In);
        base.DoInTween();
    }

    protected override void DoOutTween(List<BuildTweenData> passedBuildList)
    {
        _tweenEase = _scheme.FadeData.EaseOut;
        _tweenTime = _scheme.SetFadeTime(TweenType.Out);
        base.DoOutTween(passedBuildList);
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenStyle == TweenStyle.In || _tweenStyle == TweenStyle.InAndOut)
        {
            _targetAlpha = 1;
        }
        else
        {
            _targetAlpha = 0;
        }
    }

    protected override void OutTweenTargetSettings() => _targetAlpha = 0;

    protected override Tween DoTweenProcess(BuildTweenData item, TweenCallback callback)
    {
        return item.MyCanvasGroup.DOFade(_targetAlpha, _tweenTime)
                   .SetId($"{_tweenName}{item.Element.GetInstanceID()}")
                   .SetEase(_tweenEase)
                   .SetAutoKill(true)
                   .Play()
                   .OnComplete(callback);
    }
}
