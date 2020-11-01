using DG.Tweening;
using UnityEngine;

public abstract class BasePositionAndScale
{
    private protected readonly IPositionScaleTween _tweenData;
    private protected readonly SizeAndPositionScheme _scheme;
    private protected readonly string _id;

    private protected BasePositionAndScale(IPositionScaleTween data, string id)
    {
        _tweenData = data;
        _scheme = data.Scheme;
        _id = $"PositionOrScale{id}";
    }

    private protected Vector3 TweenEndTarget { get; set; }
    
    public void DoTween(IsActive activate)
    {
        if (_tweenData.IsPressed)
        {
            DoPressedTween();
        }
        else
        {
            DoNonePressedTween(activate);
        }
    }

    private protected abstract void DoPressedTween();

    private void DoNonePressedTween(IsActive activate)
    {
        DOTween.Kill(_id);
        
        if (activate == IsActive.Yes)
        {
            SetUpLoopOrStandardTween();
        }
        else
        {
            TweenToStartPosition(_scheme.Time);
        }
    }

    private void SetUpLoopOrStandardTween()
    {
        if (ResetToStartBeforeLoop())
        {
            TweenToStartPosition(_scheme.Time * 0.5f, TweenToEndPosition);
        }
        else
        {
            TweenToEndPosition();
        }
    }
    
    private protected abstract bool ResetToStartBeforeLoop();
    private protected abstract void TweenToStartPosition(float time, TweenCallback callback = null);
    private protected abstract void TweenToEndPosition();
}
