public partial class UITweener
{
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

    public bool Position()
    {
        foreach (var item in _buildObjectsList)
        {
            item.SetPositionTween(_positionTween);
        }

        return _positionTween != PositionTweenType.NoTween;
    }

    public bool Rotation()
    {
        foreach (var item in _buildObjectsList)
        {
            item.RotationTween = _rotationTween != RotationTweenType.NoTween;
        }

        return _rotationTween != RotationTweenType.NoTween;
    }

    public bool Scale()
    {
        bool active = _scaleTransition != ScaleTween.NoTween;
        foreach (var item in _buildObjectsList)
        {
            item.ScaleTween = active;
        }

        return active;
    }

    public bool Punch()
    {
        return _punchShakeTween == PunchShakeTween.Punch;
    }

    public bool Shake()
    {
        return _punchShakeTween == PunchShakeTween.Shake;
    }

    public bool Fade()
    {
        return _canvasGroupFade != FadeTween.NoTween;
    }

    public bool AddTweenEvent()
    {
        return _addTweenEventTriggers == IsActive.Yes;
    }
}