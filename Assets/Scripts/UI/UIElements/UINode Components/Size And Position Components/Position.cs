using DG.Tweening;
using UnityEngine;

public class Position : BasePositionAndScale, INodeTween
{
    public Position(IPositionScaleTween data, string iD) : base(data, iD)
    {
        TweenEndTarget = _tweenData.StartPosition + _tweenData.PixelsToMoveBy;
    }

    private protected override void DoPressedTween() 
        => DoPositionTween(TweenEndTarget,2, _tweenData.Time);
    
    private protected override bool ResetToStartBeforeLoop() 
        => _tweenData.CanLoop && _tweenData.MyRect.anchoredPosition3D != _tweenData.StartPosition;
    
    private protected override void TweenToStartPosition(float time, TweenCallback callback = null) 
        => DoPositionTween(_tweenData.StartPosition,0, time, callback);

    private protected override void TweenToEndPosition()
    {
        int looping = _tweenData.CanLoop ? -1 : 0;
        DoPositionTween(TweenEndTarget, looping, _tweenData.Time);
    }
    
    private void DoPositionTween (Vector3 targetPos, int loop, float time, TweenCallback callback = null)
    {
        _tweenData.MyRect.DOAnchorPos3D(targetPos, time, _tweenData.Snapping)
               .SetId(_id)
               .SetLoops(loop, LoopType.Yoyo)
               .SetEase(_tweenData.Ease)
               .SetAutoKill(true)
               .OnComplete(callback)
               .Play();
    }
}
