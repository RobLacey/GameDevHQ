﻿using DG.Tweening;
using UnityEngine;

public class Scale : BasePositionAndScale, INodeTween
{
    public Scale(IPositionScaleTween data, string iD) : base(data, iD)
    {
        TweenEndTarget = _tweenData.StartSize + _tweenData.ChangeBy;
    }
    
    private protected override void DoPressedTween() 
        => DoScaleTween(TweenEndTarget,2, _tweenData.Time);

    private protected override bool ResetToStartBeforeLoop() 
        => _tweenData.CanLoop && _tweenData.MyRect.localScale != _tweenData.StartSize;

    private protected override void TweenToStartPosition(float time, TweenCallback callback = null) 
        => DoScaleTween(_tweenData.StartSize,0, time, callback);
    
    private protected override void TweenToEndPosition()
    {
        int looping = _tweenData.CanLoop ? -1 : 0;
        DoScaleTween(TweenEndTarget, looping, _tweenData.Time);
    }
    
    private void DoScaleTween (Vector3 target, int loop, float time, TweenCallback callback = null)
    {
        _tweenData.MyRect.DOScale(target, time)
              .SetId(_id)
              .SetLoops(loop, LoopType.Yoyo)
              .SetEase(_tweenData.Ease)
              .SetAutoKill(true)
              .OnComplete(callback)
              .Play();
    }
}