using System.Collections;
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

    //Variables
    private PositionTweenType _tweenType;

    public override void SetUpTweens(List<TweenSettings> buildObjectsList, Action<RectTransform> effectCall)
    {
        _tweenName = GetType().Name;
        SetUpTweensCommon(buildObjectsList, effectCall);
        
        foreach (var uIObject in _buildList)
        {
            uIObject._element.anchoredPosition3D = uIObject._tweenStartPosition;
        }
    }

    public override void StartTween(Enum tweenType, float tweenTime, 
                                TweenType isIn, TweenCallback tweenCallback)
    {
        _tweenType = (PositionTweenType)tweenType;
        if (_tweenType == PositionTweenType.NoTween) return;
        StartTweenCommon(tweenTime, tweenCallback);

        switch (_tweenType)
        {
            case PositionTweenType.In:
                InTween(isIn);
                break;
            case PositionTweenType.Out:
                OutTween(isIn);
                break;
            case PositionTweenType.InAndOut:
                InAndOutTween(isIn);
                break;
        }
    }
    
    protected override IEnumerator TweenSequence()
    {
        bool finished = false;
        int index = 0;
        while (!finished)
        {
            foreach (var item in _listToUse)
            {
                if (index == _listToUse.Count - 1)
                {
                    Tween tween = item._element.DOAnchorPos3D(item._moveTo, _tweenTime, _pixelSnapping)
                                      .SetId($"{_tweenName}{item._element.GetInstanceID()}")
                                      .SetEase(_tweenEase)
                                      .SetAutoKill(true)
                                      .Play()
                                      .OnComplete(_callback);
                    yield return tween.WaitForCompletion();
                    _endEffectTrigger?.Invoke(item._element);
                }
                else
                {
                    item._element.DOAnchorPos3D(item._moveTo, _tweenTime, _pixelSnapping)
                                .SetId($"{_tweenName}{item._element.GetInstanceID()}")
                                .SetEase(_tweenEase)
                                .SetAutoKill(true)
                                .Play()
                                .OnComplete(() => _endEffectTrigger?.Invoke(item._element));
                    yield return new WaitForSeconds(item._buildNextAfterDelay);
                    index++;
                }
            }
            finished = true;
        }
        yield return null;
    }
    
    protected override void RewindTweens()
    {
        foreach (var item in _buildList)
        {
            item._element.anchoredPosition3D = item._tweenStartPosition;
        }
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenType == PositionTweenType.InAndOut)
        {
            foreach (var uIObject in _listToUse)
            {
                uIObject._moveTo = uIObject._tweenMiddlePosition;
            }
        }
        else
        {
            foreach (var uIObject in _listToUse)
            {
                uIObject._moveTo = uIObject._tweenTargetPosition;
            }
        }
    }

    protected override void OutTweenTargetSettings()
    {
        foreach (var uIObject in _listToUse)
        {
            uIObject._moveTo = uIObject._tweenTargetPosition;
        }
    }
}
