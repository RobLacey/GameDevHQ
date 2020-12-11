using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public interface IShakeTween: ITweenBase { }


[Serializable]
public class ShakeTween : TweenBase, IShakeTween
{
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<IEndTween>(EndTweenEffect);
    }

    public override void RemoveEvents()
    {
        base.RemoveEvents();
        EVent.Do.Unsubscribe<IEndTween>(EndTweenEffect);
    }

    public override void SetUpTweens(List<BuildTweenData> buildObjectsList, TweenScheme tweenScheme, 
                                    Action<BuildTweenData> effectCall)
    {
        base.SetUpTweens(buildObjectsList, tweenScheme, effectCall);
        foreach (var item in _buildList)
        {
            item._shakeStartScale = item.Element.localScale;
        }
    }

    public override void StartTween(TweenType tweenType, TweenCallback tweenCallback)
    {
        if (_scheme is null) return;
        _tweenStyle = _scheme.ShakeTween;
        base.StartTween(tweenType, tweenCallback);
    }
    
    protected override void RewindTweens()
    {
        foreach (var item in _buildList)
        {
            item.Element.localScale = item._shakeStartScale;
        }
    }

    protected override Tween DoTweenProcess(BuildTweenData item, TweenCallback callback)
    {
        if (_scheme.ShakeData.EndTween)
        {
            callback?.Invoke();
            return null;
        } 

        var data = _scheme.ShakeData;
        return item.Element.DOShakeScale(data.Duration, data.Strength, data.Vibrato, data.Randomness, data.FadeOut)
                           .SetId($"{_tweenName}{item.Element.GetInstanceID()}")
                           .SetAutoKill(true)
                           .Play()
                           .OnComplete(callback);
    }


    protected override void InTweenTargetSettings() => RewindTweens();

    protected override void OutTweenTargetSettings() => RewindTweens();

    public static void EndTweenEffect(IEndTween item)
    {
        if(!CanEndPunch()) return;
        
        var data = item.Scheme.ShakeData;
        item.EndTweenRect.DOShakeScale(data.Duration, data.Strength, data.Vibrato, data.Randomness, data.FadeOut)
                   .SetAutoKill(true)
                   .Play();

        bool CanEndPunch() => (item.Scheme.Shake() && item.Scheme.ShakeData.EndTween);
    }
}
