using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public interface IPunchTween: ITweenBase { }


[Serializable]
public class PunchTween : TweenBase, IPunchTween
{
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        BranchEvent.Do.Subscribe<IEndTween>(EndTweenEffect);
    }

    public override void SetUpTweens(List<BuildTweenData> buildObjectsList, 
                                     TweenScheme tweenScheme, Action<BuildTweenData> effectCall)
    {
        base.SetUpTweens(buildObjectsList, tweenScheme, null);
        
        foreach (var item in _buildList)
        {
            item._punchStartScale = item.Element.localScale;
        }
    }

    public override void StartTween(TweenType tweenType, TweenCallback tweenCallback)
    {
        if (_scheme is null) return;
        _tweenStyle = _scheme.PunchTween;
        base.StartTween(tweenType, tweenCallback);
    }
    
    protected override void RewindTweens()
    {
        foreach (var item in _buildList)
        {
            item.Element.localScale = item._punchStartScale;
        }
    }
    
    protected override Tween DoTweenProcess(BuildTweenData item, TweenCallback callback)
    {
        if (_scheme.PunchData.EndTween)
        {
            callback?.Invoke();
            return null;
        } 
        
        var data = _scheme.PunchData;
        return item.Element.DOPunchScale(data.Strength, data.Duration, data.Vibrato, data.Elasticity)
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
        
        var data = item.Scheme.PunchData;
        item.EndTweenRect.DOPunchScale(data.Strength, data.Duration, data.Vibrato, data.Elasticity)
            .SetAutoKill(true)
            .Play();

        bool CanEndPunch() => (item.Scheme.Punch() && item.Scheme.PunchData.EndTween);
    }
}
