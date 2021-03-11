using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public interface IScaleTween: ITweenBase { }

// ReSharper disable IdentifierTypo
[Serializable]
public class ScaleTween : TweenBase, IScaleTween
{
    public override void SetUpTweens(List<BuildTweenData> buildObjectsList,
                                     TweenScheme tweenScheme,
                                     Action<BuildTweenData> effectCall)
    {
        base.SetUpTweens(buildObjectsList, tweenScheme, effectCall);

        foreach (var item in _buildList)
        {
            item.Element.transform.localScale = item.ScaleSettings.StartScale;
        }
    }

    protected override Tween DoTweenProcess(BuildTweenData item, TweenCallback callback)
    {
        return item.Element.DOScale(item._scaleTo, _tweenTime)
                   .SetId($"{_tweenName}{item.Element.GetInstanceID()}")
                   .SetEase(_tweenEase)
                   .SetAutoKill(true)
                   .Play()
                   .OnComplete(callback);
    }

    public override void StartTween(TweenType tweenType, TweenCallback tweenCallback)
    {
        if (_scheme is null) return;
        
        _tweenStyle = _scheme.ScaleTween;
        base.StartTween(tweenType, tweenCallback);
    }

    protected override void RewindTweens()
    {
        foreach (var item in _buildList) 
        {
            item.Element.transform.localScale = item.ScaleSettings.StartScale;
        }
    }
    
    protected override void DoInTween()
    {
        _tweenEase = _scheme.ScaleData.EaseIn;
        _tweenTime = _scheme.SetScaleTime(TweenType.In);
        base.DoInTween();
    }

    protected override void DoOutTween(List<BuildTweenData> passedBuildList)
    {
        _tweenEase = _scheme.ScaleData.EaseOut;
        _tweenTime = _scheme.SetScaleTime(TweenType.Out);
        base.DoOutTween(passedBuildList);
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenStyle == TweenStyle.InAndOut)
        {
            foreach (var uIObject in _listToUse)
            {
                uIObject._scaleTo = uIObject.ScaleSettings.MidScale;
            }
        }
        else
        {
            foreach (var uIObject in _listToUse)
            {
                uIObject._scaleTo = uIObject.ScaleSettings.EndScale;
            }
        }
    }

    protected override void OutTweenTargetSettings()
    {
        foreach (var item in _listToUse)
        {
            item._scaleTo = item.ScaleSettings.EndScale;
        }
    }
}
