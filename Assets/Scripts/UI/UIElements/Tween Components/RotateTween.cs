using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// ReSharper disable IdentifierTypo
[Serializable]
public class RotateTween : TweenBase
{
    //Variables
    private RotationTweenType _tweenType;

    public override void SetUpTweens(List<TweenSettings> buildObjectsList, Action<RectTransform> effectCall)
    {
        _tweenName = GetType().Name;
        SetUpTweensCommon(buildObjectsList, effectCall);
        
        foreach (var item in _buildList)
        {
            item._element.localRotation = Quaternion.Euler(item.RotationSettings.StartRotation);
        }
    }

    public override void StartTween(Enum tweenType, float tweenTime, 
                                    TweenType isIn, TweenCallback tweenCallback)
    {
        _tweenType = (RotationTweenType) tweenType;
        if (_tweenType == RotationTweenType.NoTween) return;
        StartTweenCommon(tweenTime, tweenCallback);
        
        switch (_tweenType)
        {
            case RotationTweenType.In:
                InTween(isIn);
                break;
            case RotationTweenType.Out:
                OutTween(isIn);
                break;
            case RotationTweenType.InAndOut:
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
                    Tween tween = item._element.DOLocalRotate(item._targetRotation, _tweenTime)
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
                    item._element.DOLocalRotate(item._targetRotation, _tweenTime)
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
            item._element.localRotation = Quaternion.Euler(item.RotationSettings.StartRotation);
        }
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenType == RotationTweenType.InAndOut)
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
