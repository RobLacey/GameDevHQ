using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class ShakeTweener : TweenBase
{
    //Properties
    // private bool CheckInEffectType => _shakeWhen == EffectType.In || _shakeWhen == EffectType.Both;
    // private bool CheckOutEffectType => _shakeWhen == EffectType.Out || _shakeWhen == EffectType.Both;

    public override void SetUpTweens(List<BuildTweenData> buildObjectsList, TweenScheme tweenScheme, Action<RectTransform> effectCall)
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
        var data = _scheme.ShakeData;
        return item.Element.DOShakeScale(data.Duration, data.Strength, data.Vibrato, data.Randomness, data.FadeOut)
                           .SetId($"{_tweenName}{item.Element.GetInstanceID()}")
                           .SetAutoKill(true)
                           .Play()
                           .OnComplete(callback);
    }


    protected override void InTweenTargetSettings() => RewindTweens();

    protected override void OutTweenTargetSettings() => RewindTweens();

    public void EndEffect(RectTransform rectTransform/*, IsActive isIn*/)
    {
      //  /*if (isIn == IsActive.Yes) */DoEndEffectTween(rectTransform, CheckInEffectType);
    }

    // private void DoEndEffectTween(RectTransform rectTransform, bool checkForTween)
    // {
    //     if (!checkForTween) return;
    //     ResetForTween();
    //     rectTransform.DOShakeScale(_duration, _strength, _vibrato, _randomness, _fadeOut)
    //                  .SetId("shake" + rectTransform.gameObject.GetInstanceID())
    //                  .SetAutoKill(true)
    //                  .Play();
    // }
}
