using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// ReSharper disable IdentifierTypo
[Serializable]
public class RotateTween : TweenBase
{
    public override void SetUpTweens(List<BuildTweenData> buildObjectsList,
                                     TweenScheme tweenScheme,
                                     Action<RectTransform> effectCall)
    {
        base.SetUpTweens(buildObjectsList, tweenScheme, effectCall);
        
        foreach (var item in _buildList)
        {
            item.Element.localRotation = Quaternion.Euler(item.RotationSettings.StartRotation);
        }
    }

    protected override Tween DoTweenProcess(BuildTweenData item, TweenCallback callback)
    {
        return item.Element.DOLocalRotate(item._targetRotation, _tweenTime)
                   .SetId($"{_tweenName}{item.Element.GetInstanceID()}")
                   .SetEase(_tweenEase)
                   .SetAutoKill(true)
                   .Play()
                   .OnComplete(callback);
    }

    public override void StartTween(TweenType tweenType, TweenCallback tweenCallback)
    {
        if (_scheme is null) return;
        _tweenStyle = _scheme.RotationTween;
        base.StartTween(tweenType, tweenCallback);
    }

    protected override void RewindTweens()
    {
        foreach (var item in _buildList)
        {
            item.Element.localRotation = Quaternion.Euler(item.RotationSettings.StartRotation);
        }
    }
    
    protected override void DoInTween()
    {
        _tweenEase = _scheme.RotationData.EaseIn;
        _tweenTime = _scheme.SetRotationTime(TweenType.In);
        base.DoInTween();
    }

    protected override void DoOutTween(List<BuildTweenData> passedBuildList)
    {
        _tweenEase = _scheme.RotationData.EaseOut;
        _tweenTime = _scheme.SetRotationTime(TweenType.Out);
        base.DoOutTween(passedBuildList);
    }


    protected override void InTweenTargetSettings()
    {
        if (_tweenStyle == TweenStyle.InAndOut)
        {
            foreach (var item in _listToUse)
            {
                item._targetRotation = item.RotationSettings.MidPoint;
            }
        }
        else
        {
            foreach (var item in _listToUse)
            {
                item._targetRotation = item.RotationSettings.EndRotation;
            }
        }
    }

    protected override void OutTweenTargetSettings()
    {
        foreach (var item in _listToUse)
        {
            item._targetRotation = item.RotationSettings.EndRotation;
        }

    }
}
