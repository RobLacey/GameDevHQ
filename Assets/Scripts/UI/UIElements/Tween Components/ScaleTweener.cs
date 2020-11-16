using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

// ReSharper disable IdentifierTypo
[Serializable]
public class ScaleTweener : TweenBase
{
    private ScaleTween _tweenType;
    
    public override void SetUpTweens(List<TweenSettings> buildObjectsList, Action<RectTransform> effectCall)
    {
        _tweenName = GetType().Name;
        SetUpTweensCommon(buildObjectsList, effectCall);
        
        foreach (var item in _buildList)
        {
            item._element.transform.localScale = item.ScaleSettings.StartScale;
        }
    }

    public override void StartTween(Enum tweenType, float tweenTime, 
                                    TweenType isIn, TweenCallback tweenCallback)
    {
        _tweenType = (ScaleTween) tweenType;
        if (_tweenType == ScaleTween.NoTween) return;
        StartTweenCommon(tweenTime, tweenCallback);
        
        switch (_tweenType)
        {
            case ScaleTween.Scale_InOnly:
                InTween(isIn);
                break;
            case ScaleTween.Scale_OutOnly:
                OutTween(isIn);
                break;
            case ScaleTween.Scale_InAndOut:
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
                    Tween tween = item._element.DOScale(item._scaleTo, _tweenTime)
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
                    item._element.DOScale(item._scaleTo, _tweenTime)
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
            item._element.transform.localScale = item.ScaleSettings.StartScale;
        }
    }

    protected override void InTweenTargetSettings()
    {
        if (_tweenType == ScaleTween.Scale_InAndOut)
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

        // foreach (var item in _listToUse)
        // {
        //     item._scaleTo = item.ScaleSettings.TargetScale;
        // }
    }

    protected override void OutTweenTargetSettings()
    {
        foreach (var item in _listToUse)
        {
            item._scaleTo = item.ScaleSettings.EndScale;
        }

        // if (_tweenType == ScaleTween.Scale_OutOnly)
        // {
        //     foreach (var item in _listToUse)
        //     {
        //         item._scaleTo = item.ScaleSettings.EndScale;
        //     }
        // }
        // else
        // {
        //     foreach (var item in _listToUse)
        //     {
        //         item._scaleTo = item.ScaleSettings.StartScale;
        //     }
        // }
    }
    // protected override void InTweenTargetSettings()
    // {
    //     foreach (var item in _listToUse)
    //     {
    //         item._scaleTo = item.ScaleSettings.TargetScale;
    //     }
    // }
    //
    // protected override void OutTweenTargetSettings()
    // {
    //     if (_tweenType == ScaleTween.Scale_OutOnly)
    //     {
    //         foreach (var item in _listToUse)
    //         {
    //             item._scaleTo = item.ScaleSettings.TargetScale;
    //         }
    //     }
    //     else
    //     {
    //         foreach (var item in _listToUse)
    //         {
    //             item._scaleTo = item.ScaleSettings.StartScale;
    //         }
    //     }
    // }
}
