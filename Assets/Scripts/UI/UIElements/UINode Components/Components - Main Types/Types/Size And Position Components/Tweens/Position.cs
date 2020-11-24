using DG.Tweening;
using UnityEngine;

public interface IPosition : INodeTween { }


public class Position : BasePositionAndScale, IPosition
{
    public Position(IPositionScaleTween data) : base(data)
    {
        TweenEndTarget = _tweenData.StartPosition + _scheme.PixelsToMoveBy;
    }

    private protected override void DoPressedTween() 
        => DoPositionTween(TweenEndTarget,2, _scheme.Time);
    
    private protected override bool ResetToStartBeforeLoop() 
        => _scheme.CanLoop && _tweenData.MyRect.anchoredPosition3D != _tweenData.StartPosition;
    
    private protected override void TweenToStartPosition(float time, TweenCallback callback = null) 
        => DoPositionTween(_tweenData.StartPosition,0, time, callback);

    private protected override void TweenToEndPosition()
    {
        int looping = _scheme.CanLoop ? -1 : 0;
        DoPositionTween(TweenEndTarget, looping, _scheme.Time);
    }

    private void DoPositionTween (Vector3 targetPos, int loop, float time, TweenCallback callback = null)
    {
        _tweenData.MyRect.DOAnchorPos3D(targetPos, time, _scheme.Snapping)
               .SetId(_id)
               .SetLoops(loop, LoopType.Yoyo)
               .SetEase(_scheme.Ease)
               .SetAutoKill(true)
               .OnComplete(callback)
               .Play();
    }
}
