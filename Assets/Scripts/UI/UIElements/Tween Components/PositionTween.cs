using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using System;

[Serializable]
public class PositionTween : TweenBase
{
    [SerializeField] [AllowNesting] bool _pixelSnapping;

    public override void SetUpTweens(List<BuildTweenData> buildObjectsList,
                                     TweenScheme tweenScheme,
                                     Action<RectTransform> effectCall)
    {
        base.SetUpTweens(buildObjectsList, tweenScheme, effectCall);
        
        foreach (var uIObject in _buildList)
        {
            uIObject.Element.anchoredPosition3D = uIObject.PositionSettings.StartPos;
        }
    }

    public override void StartTween(TweenType tweenType, TweenCallback tweenCallback)
    {
        if (_scheme is null) return;
        _tweenStyle = _scheme.PositionTween;
        base.StartTween(tweenType, tweenCallback);
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

    protected override void DoInTween()
    {
        _tweenEase = _scheme.PositionData.EaseIn;
        _tweenTime = _scheme.SetPositionTime(TweenType.In);
        base.DoInTween();
    }

    protected override void DoOutTween(List<BuildTweenData> passedBuildList)
    {
        _tweenEase = _scheme.PositionData.EaseOut;
        _tweenTime = _scheme.SetPositionTime(TweenType.Out);
        base.DoOutTween(passedBuildList);
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenStyle == TweenStyle.InAndOut)
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
