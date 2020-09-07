using System;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;

[RequireComponent(typeof(Transform))]
[Serializable]
public class UISizeAndPosition : NodeFunctionBase
{
    [SerializeField] private Choose _changeSizeOn = Choose.None;
    [SerializeField] [AllowNesting] [ShowIf("Activate")] private ScaleType _scaledType = ScaleType.ScaleTween;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] private float _punchTime = 0.5f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] private float _punchScaleByX = 0.1f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] private float _punchScaleByY = 0.2f;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] private int _punchVibrato = 6;
    [SerializeField] [AllowNesting] [ShowIf("IsPunch")] private float _elasticity = 0.5f;
    
    [SerializeField] [AllowNesting] [ShowIf("IsShake")]private float _shakeScaleByX = 0.1f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")]private float _shakeScaleByY = 0.2f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")]private float _shakeTime = 0.5f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")]private float _shakeRandomness = 45f;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")]private int _shakeVibrato = 6;
    [SerializeField] [AllowNesting] [ShowIf("IsShake")]private bool _fadeOut = true;
    
    [SerializeField] [AllowNesting] [ShowIf("IsPositionTween")]private Vector3 _pixelsToMoveBy;
    [SerializeField] [AllowNesting] [ShowIf("IsScaleTween")]private Vector3 _scaleBy;
    [SerializeField] [AllowNesting] [ShowIf("IsTween")]private float _time;
    [SerializeField] [AllowNesting] [ShowIf("IsTween")]private Ease _ease;
    [SerializeField] [AllowNesting] [HideIf("DontAllowLoop")]private bool _loop;
    [SerializeField] [AllowNesting] [ShowIf("IsPositionTween")] private bool _snapping;

    //Variables
    private string _scaleId;
    private INodeTween _tween;

    //Properties
    public bool IsPressed { get; private set; }
    public RectTransform MyRect { get; private set; }
    public Transform MyTransform { get; private set; }
    public Vector3 StartSize { get; private set; }
    public Vector3 StartPosition { get; private set; }
    public bool CanLoop => _loop;
    public int GameObjectID { get; private set; }

    public (float time, float scaleXBy, float scaleYBy, int vibrato, float elasticity) 
        PunchData { get; private set; }

    public (float time, float scaleXBy, float scaleYBy, int vibrato, float randomness, bool fadeOut) 
        ShakeData { get; private set; }
    public (float time, Vector3 pixelsToMoveBy, Ease ease, bool snapping) 
        PositionData { get; private set; }
    protected override bool CanBeSelected() => false;
    protected override bool CanBeHighlighted() => _changeSizeOn == Choose.Highlighted 
                                                  || _changeSizeOn == Choose.HighlightedAndSelected;
    protected override bool CanBePressed() => _changeSizeOn == Choose.Pressed || _changeSizeOn == Choose.Selected; 
    protected override bool FunctionNotActive() => !CanActivate;

    //Editor Script

    #region Editor Scripts

    public bool Activate() => _changeSizeOn != Choose.None ;
    public bool IsPunch() => _scaledType == ScaleType.ScalePunch && _changeSizeOn != Choose.None;
    public bool IsShake() => _scaledType == ScaleType.ScaleShake && _changeSizeOn != Choose.None;
    public bool IsTween() => _scaledType == ScaleType.PositionTween ||  _scaledType == ScaleType.ScaleTween 
                                            && _changeSizeOn != Choose.None;
    public bool IsPositionTween() => _scaledType == ScaleType.PositionTween && _changeSizeOn != Choose.None;
    public bool IsScaleTween() => _scaledType == ScaleType.ScaleTween && _changeSizeOn != Choose.None;
    public bool DontAllowLoop() => _changeSizeOn == Choose.Pressed;

    #endregion


    public void OnAwake(UINode node, UiActions uiActions)
    {
        base.OnAwake(node, uiActions);
        if (DontAllowLoop()) _loop = false;
        CanActivate = (_enabledFunctions & Setting.SizeAndPosition) != 0;
        GameObjectID = node.gameObject.GetInstanceID();
        SetTweenIds();
        MyRect = node.GetComponent<RectTransform>();
        MyTransform = node.transform;
        StartSize = MyRect.localScale;
        StartPosition = MyRect.anchoredPosition3D;
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        base.SavePointerStatus(pointerOver);
        if(!CanBeHighlighted()) return;
        
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
        if(!CanBePressed()) return;
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

    private protected override void ProcessSelectedAndHighLighted() { }

    private protected override void ProcessHighlighted() {  }

    private protected override void ProcessSelected() { }

    private protected override void ProcessToNormal()  {  }

    private protected override void ProcessDisabled(bool isDisabled) { }

    private void SetTweenIds()
    {
        if(_scaledType == ScaleType.ScalePunch)
        {
            PunchData = (_punchTime, _punchScaleByX, _punchScaleByY, _punchVibrato, _elasticity);
            _tween = new Punch(this);
        } 
        
        if(_scaledType == ScaleType.ScaleShake)
        {
            ShakeData = (_shakeTime, _shakeScaleByX, _shakeScaleByY, _shakeVibrato, _shakeRandomness, _fadeOut);
            _tween = new Shake(this);
        } 
        
        if(_scaledType == ScaleType.PositionTween)
        {
            PositionData = (_time, _pixelsToMoveBy, _ease, _snapping);
            _tween = new Position(this);
        }        
        _scaleId = $"scale{GameObjectID}";
    }


    // private bool IsADeactivatePress(IsActive activate)
    // {
    //     return (_changeSizeOn == Choose.Selected || _changeSizeOn == Choose.HighlightedAndSelected) && 
    //         activate == IsActive.No;
    // }

    private void ScalePressed(IsActive isSelected)
    {
        if (_changeSizeOn == Choose.Pressed)
        {
            DoScaleTween(StartSize + _scaleBy, 2);
        }
        else if (_changeSizeOn == Choose.Selected)
        {
            ScaleTo(isSelected);
        }
    }

    private void ScaleTo(IsActive moveOut)
    {
        DOTween.Kill(_scaleId);
        int loopingCycles = _loop ? -1 : 0;
    
        if (moveOut == IsActive.Yes)
        {
            Vector3 targetPos = StartSize + _scaleBy;
            DoScaleTween(targetPos, loopingCycles);
        }
        else
        {
            Vector3 targetPos = StartSize;
            DoScaleTween(targetPos, 0);
        }
    }


    private void DoScaleTween (Vector3 target, int loop)
    {
        MyRect.DOScale(target, _time)
               .SetId(_scaleId)
               .SetLoops(loop, LoopType.Yoyo)
               .SetEase(_ease)
               .SetAutoKill(true)
               .Play();
    }
}
