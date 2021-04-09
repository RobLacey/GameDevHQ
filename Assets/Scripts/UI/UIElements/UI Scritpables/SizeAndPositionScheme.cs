using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "SizeAndPosScheme", menuName = "UIElements Schemes / New Size And Position Scheme")]

public class SizeAndPositionScheme : ScriptableObject
{
    [Header("Size And Position Tween", order = 2)] 
    [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 3)]

    [SerializeField] 
    private Choose _changeSizeOn = Choose.None;
    
    [SerializeField] 
    private TweenEffect _tweenEffect = TweenEffect.Scale;

    [Header("Settings", order = 2)] 
    [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 3)]

    [SerializeField] 
    [Range(0f, 5f)] [Label("Tween Time")]
    private float _time = 0.2f;

    [SerializeField] 
    [ShowIf("PositionSettings")] 
    private Vector3 _pixelsToMoveBy = default;

    [SerializeField] 
    [ShowIf("OtherSettings")] [Label("Scale, Punch or Shake By")]
    private Vector3 _changeBy = default;

    [SerializeField] 
    [HideIf("DontAllowLoop")] [Label("Loop Tween While Active")]
    private bool _loop = false;
    
    [SerializeField] 
    [ShowIf("IsPunchOrShake")] [Range(0f, 15f)] 
    private int _vibrato = 6;
    
    [SerializeField] 
    [ShowIf("IsPunch")] [Range(0f, 1f)]  
    private float _elasticity = 0.5f;
    
    [SerializeField] 
    [ShowIf("IsShake")] [Range(0f, 90f)]  
    private float _shakeRandomness = 45f;
    
    [SerializeField] 
    [ShowIf("IsShake")] 
    private bool _fadeOut = true;
    
    [SerializeField] 
    [ShowIf("ShowEase")] [Label("Ease Type")]
    private Ease _ease = Ease.Linear;
    

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
    public bool CanBeSelectedAndHighlight => ChangeSizeOn == Choose.HighlightedAndSelected;
    public bool CanBeSelected => ChangeSizeOn == Choose.Selected;
    public bool IsPressed => ChangeSizeOn == Choose.Pressed;
    public bool CanBeHighlighted => ChangeSizeOn == Choose.Highlighted;
    public bool NotSet => ChangeSizeOn == Choose.None;

    private bool IsPunch() => _tweenEffect == TweenEffect.Punch && _changeSizeOn != Choose.None;
    private bool IsShake() => _tweenEffect == TweenEffect.Shake && _changeSizeOn != Choose.None;
    private bool DontAllowLoop() => _changeSizeOn == Choose.Pressed || _changeSizeOn == Choose.None;

    //Editor Scripts
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