using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "SizeAndPosScheme", menuName = "UIElements Schemes / New Size And Position Scheme")]

public class SizeAndPositionScheme : ScriptableObject
{
    [SerializeField] private Choose _changeSizeOn = Choose.None;
    [SerializeField] [ShowIf("Activate")] private TweenEffect _tweenEffect = TweenEffect.Scale;
    [SerializeField] [ShowIf("PositionSettings")] private Vector3 _pixelsToMoveBy;
    [SerializeField] [ShowIf("OtherSettings")] private Vector3 _changeBy;
    [SerializeField] [ShowIf("IsTween")] [Range(0f, 5f)] private float _time;
    [SerializeField] [HideIf("DontAllowLoop")] private bool _loop;
    [SerializeField] [ShowIf("IsPunchOrShake")] [Range(0f, 15f)] private int _vibrato = 6;
    [SerializeField] [ShowIf("IsPunch")] [Range(0f, 1f)]  private float _elasticity = 0.5f;
    [SerializeField] [ShowIf("IsShake")] [Range(0f, 90f)]  private float _shakeRandomness = 45f;
    [SerializeField] [ShowIf("IsShake")] private bool _fadeOut = true;
    [SerializeField] [ShowIf("PositionSettings")] private bool _snapping;
    [SerializeField] [ShowIf("ShowEase")] private Ease _ease;

    private Choose ChangeSizeOn => _changeSizeOn;
    public TweenEffect TweenEffect => _tweenEffect;
    public bool CanLoop => _loop;
    public Vector3 PixelsToMoveBy => _pixelsToMoveBy;
    public Vector3 ChangeBy => _changeBy;
    public float Time => _time;
    public Ease Ease => _ease;
    public int Vibrato => _vibrato;
    public float Elasticity => _elasticity;
    public float Randomness => _shakeRandomness;
    public bool FadeOut => _fadeOut;
    public bool Snapping => _snapping;
    public bool CanBeSelectedAndHighlight => ChangeSizeOn == Choose.HighlightedAndSelected;
    public bool CanBeSelected => ChangeSizeOn == Choose.Selected;
    public bool IsPressed => ChangeSizeOn == Choose.Pressed;
    public bool CanBeHighlighted => ChangeSizeOn == Choose.Highlighted;
    public bool NotSet => ChangeSizeOn == Choose.None;

    private bool IsPunch() => _tweenEffect == TweenEffect.Punch && _changeSizeOn != Choose.None;
    private bool IsShake() => _tweenEffect == TweenEffect.Shake && _changeSizeOn != Choose.None;
    private bool DontAllowLoop() => _changeSizeOn == Choose.Pressed || _changeSizeOn == Choose.None;

    //Editor Scripts
    public bool Activate() => _changeSizeOn != Choose.None;
    public bool IsTween() => _changeSizeOn != Choose.None;
    public bool PositionSettings() => _tweenEffect == TweenEffect.Position && _changeSizeOn != Choose.None;
    public bool OtherSettings() => _tweenEffect != TweenEffect.Position && _changeSizeOn != Choose.None;
    public bool IsScaleTween() => _tweenEffect == TweenEffect.Scale && _changeSizeOn != Choose.None;
    public bool IsPunchOrShake() => IsShake() || IsPunch();
    public bool ShowEase() => !IsShake() && !IsPunch() && _changeSizeOn != Choose.None;

    public void OnAwake()
    {
        if (DontAllowLoop()) _loop = false;
    }

}