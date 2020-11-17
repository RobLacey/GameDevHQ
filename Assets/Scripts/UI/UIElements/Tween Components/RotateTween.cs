using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// ReSharper disable IdentifierTypo
[Serializable]
public class RotateTween : TweenBase
{
    public override void SetUpTweens(List<BuildTweenData> buildObjectsList, 
                                     Action<RectTransform> effectCall)
    {
        base.SetUpTweens(buildObjectsList, effectCall);
        
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

    protected override void RewindTweens()
    {
        foreach (var item in _buildList)
        {
            item.Element.localRotation = Quaternion.Euler(item.RotationSettings.StartRotation);
        }
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenType == TweenStyle.InAndOut)
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
