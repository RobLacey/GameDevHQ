using UnityEngine;

public partial class UITweener
{
    // public bool Punch() => _punchShakeTween == PunchShakeTween.Punch;//**
    //
    // public bool Shake() => _punchShakeTween == PunchShakeTween.Shake;//**
    //
    // public bool Fade() => _canvasGroupFade != TweenStyle.NoTween;//**
    //
    // public bool AddTweenEvent() => _addTweenEventTriggers == IsActive.Yes;//**
    //
    // private bool Position() => _positionTween != TweenStyle.NoTween;//**
    //
    // private bool Rotation() => _rotationTween != TweenStyle.NoTween;//**
    //
    // private bool Scale() => _scaleTransition != TweenStyle.NoTween;//**
    //
    // public bool GlobalTime()//**
    // {
    //     bool temp;
    //     if (_useGlobalTime == IsActive.Yes)
    //     {
    //         _posTween.UsingGlobalTime = true;
    //         _scaleTween.UsingGlobalTime = true;
    //         _fadeTween.UsingGlobalTime = true;
    //         _rotateTween.UsingGlobalTime = true;
    //         temp = true;
    //     }
    //     else
    //     {
    //         _posTween.UsingGlobalTime = false;
    //         _scaleTween.UsingGlobalTime = false;
    //         _fadeTween.UsingGlobalTime = false;
    //         _rotateTween.UsingGlobalTime = false;
    //         temp = false;
    //     }
    //
    //     return temp;
    // }

    private void IsPositionSet(TweenStyle tweenStyle)
    {
        foreach (var item in _buildObjectsList)
        {
            item.PositionSettings.SetPositionTween(tweenStyle);
        }
    }

    private void IsRotationSet(TweenStyle tweenStyle)
    {
        foreach (var item in _buildObjectsList)
        {
            item.RotationSettings.SetRotationTween(tweenStyle);
        }
    }

    private void IsScaleSet(TweenStyle tweenStyle)
    {
        foreach (var item in _buildObjectsList)
        {
            item.ScaleSettings.SetScaleTween(tweenStyle);
        }
    }
}