public partial class UISizeAndPosition
{
    public bool Activate() => _changeSizeOn != Choose.None;
    public bool IsPunch() => _tweenEffect == TweenEffect.Punch && _changeSizeOn != Choose.None;
    public bool IsShake() => _tweenEffect == TweenEffect.Shake && _changeSizeOn != Choose.None;
    public bool IsTween() => _changeSizeOn != Choose.None;
    public bool PositionSettings() => _tweenEffect == TweenEffect.Position && _changeSizeOn != Choose.None;
    public bool OtherSettings() => _tweenEffect != TweenEffect.Position && _changeSizeOn != Choose.None;
    public bool IsScaleTween() => _tweenEffect == TweenEffect.Scale && _changeSizeOn != Choose.None;
    public bool IsPunchOrShake() => IsShake() || IsPunch();
    public bool ShowEase() => !IsShake() && !IsPunch() && _changeSizeOn != Choose.None;
}