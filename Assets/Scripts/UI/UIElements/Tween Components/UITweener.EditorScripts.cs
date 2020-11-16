using UnityEngine;

public partial class UITweener
{
    public bool Punch() => _punchShakeTween == PunchShakeTween.Punch;

    public bool Shake() => _punchShakeTween == PunchShakeTween.Shake;

    public bool Fade() => _canvasGroupFade != FadeTween.NoTween;

    public bool AddTweenEvent() => _addTweenEventTriggers == IsActive.Yes;

    public bool Position() => _positionTween != PositionTweenType.NoTween;

    public bool Rotation() => _rotationTween != RotationTweenType.NoTween;

    public bool Scale() => _scaleTransition != ScaleTween.NoTween;

    public bool GlobalTime()
    {
        bool temp;
        if (_useGlobalTime == IsActive.Yes)
        {
            _posTween.UsingGlobalTime = true;
            _scaleTween.UsingGlobalTime = true;
            _fadeTween.UsingGlobalTime = true;
            _rotateTween.UsingGlobalTime = true;
            temp = true;
        }
        else
        {
            _posTween.UsingGlobalTime = false;
            _scaleTween.UsingGlobalTime = false;
            _fadeTween.UsingGlobalTime = false;
            _rotateTween.UsingGlobalTime = false;
            temp = false;
        }

        return temp;
    }

    private void IsPositionSet()
    {
        //if (!Position()) return;
        foreach (var item in _buildObjectsList)
        {
            item.PositionSettings.SetPositionTween(_positionTween);
        }
    }

    private void IsRotationSet()
    {
        //if(!Rotation()) return;
        foreach (var item in _buildObjectsList)
        {
            item.RotationSettings.SetRotationTween(_rotationTween);
        }
    }

    private void IsScaleSet()
    {
        //if(!Scale()) return;
        foreach (var item in _buildObjectsList)
        {
            item.ScaleSettings.SetScaleTween(_scaleTransition);
        }
    }
}