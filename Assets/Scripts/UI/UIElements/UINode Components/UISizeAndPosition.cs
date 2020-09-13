using System;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;

[RequireComponent(typeof(Transform))]
[Serializable]
public partial class UISizeAndPosition : NodeFunctionBase, IPositionScaleTween, IPunchShakeTween
{
    [SerializeField] private Choose _changeSizeOn = Choose.None;
    [SerializeField] [AllowNesting] [ShowIf("Activate")] private TweenEffect _tweenEffect = TweenEffect.Scale;
    [SerializeField] [AllowNesting] [ShowIf("PositionSettings")] private Vector3 _pixelsToMoveBy;
    [SerializeField] [AllowNesting] [ShowIf("OtherSettings")] private Vector3 _changeBy;
    [SerializeField] [AllowNesting] [ShowIf("IsTween")] private float _time;
    [SerializeField] [AllowNesting] [HideIf("DontAllowLoop")] private bool _loop;
    [SerializeField] [AllowNesting] [ShowIf("IsPunchOrShake")] private int _vibrato = 6;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] private float _elasticity = 0.5f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] private float _shakeRandomness = 45f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")] private bool _fadeOut = true;
    [SerializeField] [AllowNesting] [ShowIf("PositionSettings")] private bool _snapping;
    [SerializeField] [AllowNesting] [ShowIf("ShowEase")] private Ease _ease;

    //Variables
    private INodeTween _tween;
    
    //Properties
    public bool IsPressed { get; private set; }
    public RectTransform MyRect { get; private set; }
    public Transform MyTransform { get; private set; }
    public int GameObjectID { get; private set; }
    public Vector3 StartPosition { get; private set; }
    public Vector3 StartSize { get; private set; }
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
    public bool DontAllowLoop() => _changeSizeOn == Choose.Pressed || _changeSizeOn == Choose.None;

    protected override bool CanBeHighlighted() => _changeSizeOn == Choose.Highlighted 
                                                  || _changeSizeOn == Choose.HighlightedAndSelected;
    protected override bool CanBePressed() => _changeSizeOn == Choose.Pressed || _changeSizeOn == Choose.Selected; 
    protected override bool FunctionNotActive() => !CanActivate && _changeSizeOn != Choose.None;

    public void OnAwake(UINode node, UiActions uiActions)
    {
        base.OnAwake(node, uiActions);
        CanActivate = (_enabledFunctions & Setting.SizeAndPosition) != 0;
        if (DontAllowLoop()) _loop = false;
        SetVariables(node);
        SetTweenIds();
    }

    private void SetVariables(UINode node)
    {
        GameObjectID = node.gameObject.GetInstanceID();
        MyRect = node.GetComponent<RectTransform>();
        MyTransform = node.transform;
        StartSize = MyRect.localScale;
        StartPosition = MyRect.anchoredPosition3D;
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive()|| !CanBeHighlighted() || !CanActivate) return;
        
        if(pointerOver)
        {
            if (_isSelected && _changeSizeOn == Choose.HighlightedAndSelected) return;
            _tween.DoTween(IsActive.Yes);
        }
        else
        {
            if (_isSelected && _changeSizeOn == Choose.HighlightedAndSelected) return;
            _tween.DoTween(IsActive.No);
        }    
    }

    private protected override void ProcessPress()
    {
        if(FunctionNotActive() ||!CanBePressed() || !CanActivate) return;
        if(_changeSizeOn == Choose.Pressed)
        {
            IsPressed = true;
            _tween.DoTween(IsActive.Yes);
            IsPressed = false;
        }
        else if(_changeSizeOn == Choose.Selected)
        {
            _tween.DoTween(_isSelected ? IsActive.Yes : IsActive.No);
        }
    }

    private protected override void ProcessDisabled(bool isDisabled) { }

    private void SetTweenIds()
    {
        switch (_tweenEffect)
        {
            case TweenEffect.Punch:
                _tween = new Punch(this, GameObjectID.ToString());
                break;
            case TweenEffect.Shake:
                _tween = new Shake(this, GameObjectID.ToString());
                break;
            case TweenEffect.Position:
                _tween = new Position(this, GameObjectID.ToString());
                break;
            case TweenEffect.Scale:
                _tween = new Scale(this, GameObjectID.ToString());
                break;
        }
    }
}
