using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

// ReSharper disable IdentifierTypo
[Serializable]
public class ScaleTweener : TweenBase
{
    public override void SetUpTweens(List<BuildTweenData> buildObjectsList, 
                                     Action<RectTransform> effectCall)
    {
        base.SetUpTweens(buildObjectsList, effectCall);

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

    protected override void RewindTweens()
    {
        foreach (var item in _buildList) 
        {
            item.Element.transform.localScale = item.ScaleSettings.StartScale;
        }
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenType == TweenStyle.InAndOut)
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
