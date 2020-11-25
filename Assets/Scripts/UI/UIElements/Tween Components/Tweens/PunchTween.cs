using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public interface IPunchTween: ITweenBase { }


[Serializable]
public class PunchTween : TweenBase, IPunchTween
{
    //Properties
    // public bool CheckInEffectType => _punchWhen == EffectType.In || _punchWhen == EffectType.Both;
    // public bool CheckOutEffectType => _punchWhen == EffectType.Out || _punchWhen == EffectType.Both;

    public override void SetUpTweens(List<BuildTweenData> buildObjectsList, 
                                     TweenScheme tweenScheme, Action<RectTransform> effectCall)
    {
        base.SetUpTweens(buildObjectsList, tweenScheme, effectCall);
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
        var data = _scheme.PunchData;
        return item.Element.DOPunchScale(data.Strength, data.Duration, data.Vibrato, data.Elasticity)
                            .SetId($"{_tweenName}{item.Element.GetInstanceID()}")
                            .SetAutoKill(true)
                            .Play()
                            .OnComplete(callback);
    }

    protected override void InTweenTargetSettings() => RewindTweens();

    protected override void OutTweenTargetSettings() => RewindTweens();

    public void EndEffect(RectTransform rectTransform/*, IsActive isIn*/)
    {
        Debug.Log("Here");
        ///*if (isIn == IsActive.Yes) */DoEndEffect(rectTransform, CheckInEffectType);
    }

    // private void DoEndEffect(RectTransform rectTransform, bool checkTweenType)
    // {
    //     if (!checkTweenType) return;
    //     ResetForTweens();
    //     rectTransform.DOPunchScale(_strength, _duration, _vibrato, _elasticity)
    //                  .SetId("punch" + rectTransform.gameObject.GetInstanceID())
    //                  .SetAutoKill(true)
    //                  .Play();
    // }
}
