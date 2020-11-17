using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using System;

// ReSharper disable IdentifierTypo
[Serializable]
public class PositionTween : TweenBase
{
    [SerializeField] [AllowNesting] bool _pixelSnapping;

    public override void SetUpTweens(List<BuildTweenData> buildObjectsList, 
                                     Action<RectTransform> effectCall)
    {
        base.SetUpTweens(buildObjectsList, effectCall);
        
        foreach (var uIObject in _buildList)
        {
            uIObject.Element.anchoredPosition3D = uIObject.PositionSettings.StartPos;
        }
    }
    
    protected override Tween DoTweenProcess(BuildTweenData item, TweenCallback callback)
    {
        return item.Element.DOAnchorPos3D(item._moveTo, _tweenTime, _pixelSnapping)
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
            item.Element.anchoredPosition3D = item.PositionSettings.StartPos;
        }
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenType == TweenStyle.InAndOut)
        {
            foreach (var uIObject in _listToUse)
            {
                uIObject._moveTo = uIObject.PositionSettings.MidPos;
            }
        }
        else
        {
            foreach (var uIObject in _listToUse)
            {
                uIObject._moveTo = uIObject.PositionSettings.EndPos;
            }
        }
    }

    protected override void OutTweenTargetSettings()
    {
        foreach (var uIObject in _listToUse)
        {
            uIObject._moveTo = uIObject.PositionSettings.EndPos;
        }
    }
}
