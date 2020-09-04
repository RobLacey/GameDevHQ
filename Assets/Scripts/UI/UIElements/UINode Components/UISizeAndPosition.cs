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
    [SerializeField] [AllowNesting] [ShowIf("CantLoop")]private bool _loop;
    [SerializeField] [AllowNesting] [ShowIf("IsPositionTween")] private bool _snapping;

    //Variables
    private RectTransform _myRect;
    private Vector3 _startPosition;
    private string _positionId;
    private string _scaleId;
    private INodeTween _tween;

    //Properties
    public Transform MyTransform { get; private set; }
    public Vector3 StartSize { get; private set; }
    public bool CanLoop => _loop;
    public int GameObjectID { get; private set; }

    public (float time, float scaleXBy, float scaleYBy, int vibrato, float elasticity) 
        PunchData { get; private set; }

    public (float time, float scaleXBy, float scaleYBy, int vibrato, float randomness, bool fadeOut) 
        ShakeData { get; private set; }
    protected override bool CanBeSelected() => false;
    protected override bool CanBeHighlighted() => _changeSizeOn == Choose.Highlighted 
                                                  || _changeSizeOn == Choose.HighlightedAndSelected;
    protected override bool CanBePressed() => _changeSizeOn == Choose.Pressed || _changeSizeOn == Choose.Selected; 
    protected override bool FunctionNotActive() => !CanActivate;

    //Editor Script

    #region Editor Scripts

    public bool Activate() => _changeSizeOn != Choose.None ;

    public bool IsStandard() => _changeSizeOn != Choose.None && _scaledType != ScaleType.ScalePunch 
                                                             && _scaledType != ScaleType.ScaleShake && _scaledType != ScaleType.PositionTween
                                                             && _scaledType != ScaleType.ScaleTween;

    public bool IsPunch() => _scaledType == ScaleType.ScalePunch && _changeSizeOn != Choose.None;
    public bool IsShake() => _scaledType == ScaleType.ScaleShake && _changeSizeOn != Choose.None;

    public bool IsTween() => _scaledType == ScaleType.PositionTween ||  _scaledType == ScaleType.ScaleTween && 
                             _changeSizeOn != Choose.None;

    public bool IsPositionTween() => _scaledType == ScaleType.PositionTween && _changeSizeOn != Choose.None;
    public bool IsScaleTween() => _scaledType == ScaleType.ScaleTween && _changeSizeOn != Choose.None;
    public bool CantLoop() => _changeSizeOn == Choose.HighlightedAndSelected  || _changeSizeOn == Choose.Pressed;

    #endregion


    public void OnAwake(UINode node, UiActions uiActions)
    {
        base.OnAwake(node, uiActions);
        if (CantLoop()) _loop = false;
        CanActivate = (_enabledFunctions & Setting.SizeAndPosition) != 0;
        GameObjectID = node.gameObject.GetInstanceID();
        SetTweenIds();
        _myRect = node.GetComponent<RectTransform>();
        MyTransform = node.transform;
        StartSize = _myRect.localScale;
        _startPosition = _myRect.anchoredPosition3D;
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        base.SavePointerStatus(pointerOver);
        if(pointerOver && CanBeHighlighted())
            _tween.DoTween(IsActive.Yes);
        
        if(!pointerOver && CanBeHighlighted())
            _tween.DoTween(IsActive.No);
    }

    private protected override void ProcessPress()
    {
        if(!CanBePressed()) return;
        _tween.DoTween(IsActive.Yes);
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
            PunchData = (_time, _punchScaleByX, _punchScaleByY, _punchVibrato, _elasticity);
            _tween = new Punch(this);
        } 
        
        if(_scaledType == ScaleType.ScaleShake)
        {
            ShakeData = (_time, _shakeScaleByX, _shakeScaleByY, _shakeVibrato, _shakeRandomness, _fadeOut);
            _tween = new Shake(this);
        }        
        //_punchId = $"punch{GameObjectID}";
        //_shakeId = $"shake{GameObjectID}";
        _positionId = $"position{GameObjectID}";
        _scaleId = $"scale{GameObjectID}";
    }

    // public Action<UIEventTypes, bool> OnDisable()
    // {
    //     return HowToChangeSize;
    // }
    //
    // private void HowToChangeSize(UIEventTypes uIEventTypes, bool selected)
    // {
    //     if (!CanActivate) return;
    //     if (_changeSizeOn == Choose.Pressed) return;
    //
    //     if (_changeSizeOn == Choose.HighlightedAndSelected)
    //     {
    //         ProcessHighlightedAndSelected(uIEventTypes, selected);
    //     }
    //     else if (uIEventTypes == UIEventTypes.Highlighted && _changeSizeOn == Choose.Highlighted)
    //     {
    //         ProcessHighlighted(IsActive.Yes);
    //     }
    //     else if (uIEventTypes == UIEventTypes.Normal)
    //     {
    //         ProcessHighlighted(IsActive.No);
    //     }
    // }

    // private void ProcessHighlightedAndSelected(UIEventTypes uIEventTypes, bool selected)
    // {
    //     if (uIEventTypes == UIEventTypes.Highlighted)
    //     {
    //         ProcessHighlighted(IsActive.Yes);            
    //     }
    //
    //     if (uIEventTypes == UIEventTypes.Normal && !selected)
    //     {
    //         ProcessHighlighted(IsActive.No);
    //     }
    // }

    // private void ProcessHighlighted(IsActive activate)
    // {
    //     if (_scaledType == ScaleType.ScalePunch)
    //     {
    //         _tween.DoTween(activate);
    //         // if (activate == IsActive.Yes)
    //         // {
    //         //     PunchTo();
    //         // }
    //         // else
    //         // {
    //         //     DOTween.Kill(_punchId);
    //         // }
    //     }
    //     else if(_scaledType == ScaleType.ScaleShake)
    //     {
    //         _tween.DoTween(activate);
    //         // if (activate == IsActive.Yes)
    //         // {
    //         //     ShakeTo();
    //         // }
    //         // else
    //         // {
    //         //     DOTween.Kill(_shakeId);
    //         // }      
    //     }
    //     else if (_scaledType == ScaleType.PositionTween)
    //     {
    //         MovePositionTo(activate);
    //     }
    //     else
    //     {
    //         ScaleTo(activate);
    //     }
    // }

    // public void WhenPressed(bool isSelected)
    // {
    //     var activate = isSelected ? IsActive.Yes : IsActive.No;
    //     
    //     if (CanActivate && PressedOrSelected)
    //     {
    //         switch (_scaledType)
    //         {
    //             case ScaleType.ScalePunch:
    //                 if (IsADeactivatePress(activate)) return;
    //                 
    //                 
    //                 //PunchTo();
    //                 break;
    //             case ScaleType.ScaleShake:
    //                 if (IsADeactivatePress(activate)) return;
    //                 //ShakeTo();
    //                 break;
    //             case ScaleType.PositionTween:
    //                 PositionPressed(activate);
    //                 break;
    //             case ScaleType.ScaleTween:
    //                 ScalePressed(activate);
    //                 break;
    //         }
    //     }
    // }

    private bool IsADeactivatePress(IsActive activate)
    {
        return (_changeSizeOn == Choose.Selected || _changeSizeOn == Choose.HighlightedAndSelected) && 
            activate == IsActive.No;
    }

    private void PositionPressed(IsActive isSelected)
    {
        if (_changeSizeOn == Choose.Pressed)
        {
            DoPositionTween(_startPosition + _pixelsToMoveBy, 2);
        }
        else if (_changeSizeOn == Choose.Selected || _changeSizeOn == Choose.HighlightedAndSelected)
        {
            MovePositionTo(isSelected);
        }
    }

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

    private void MovePositionTo(IsActive moveOut)
    {
        DOTween.Kill(_positionId);
        int loopingCycles = _loop ? -1 : 0;

        if (moveOut == IsActive.Yes)
        {
            Vector3 targetPos = _startPosition + _pixelsToMoveBy;
            DoPositionTween(targetPos, loopingCycles);
        }
        else
        {
            Vector3 targetPos = _startPosition;
            DoPositionTween(targetPos, 0);
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

    // private void PunchTo()
    // {
    //     int loopTime = 0;
    //     if (_loop)  { loopTime = -1; }
    //
    //     DOTween.Kill(_punchId);
    //     _myTransform.localScale = _startSize;
    //     Vector3 scaleBy = new Vector3(_punchScaleByX, _punchScaleByY, 0);
    //     _myTransform.DOPunchScale(scaleBy, _punchTime, _punchVibrato, _elasticity)
    //                 .SetId(_punchId)
    //                 .SetLoops(loopTime, LoopType.Yoyo)
    //                 .SetAutoKill(true)
    //                 .Play();
    // }

    // private void ShakeTo()
    // {
    //     int loopTime = 0;
    //     if (_loop)  { loopTime = -1; }
    //
    //     DOTween.Kill(_shakeId);
    //     MyTransform.localScale = StartSize;
    //     Vector3 scaleBy = new Vector3(_shakeScaleByX, _shakeScaleByY, 0);
    //     MyTransform.DOShakeScale(_shakeTime, scaleBy, _shakeVibrato, _shakeRandomness, _fadeOut)
    //                 .SetId(_shakeId)
    //                 .SetLoops(loopTime, LoopType.Yoyo)
    //                 .SetAutoKill(true)
    //                 .Play();
    // }

    private void DoPositionTween (Vector3 target, int loop)
    {
        _myRect.DOAnchorPos3D(target, _time, _snapping)
               .SetId(_positionId)
               .SetLoops(loop, LoopType.Yoyo)
               .SetEase(_ease)
               .SetAutoKill(true)
               .Play();
    }

    private void DoScaleTween (Vector3 target, int loop)
    {
        _myRect.DOScale(target, _time)
               .SetId(_scaleId)
               .SetLoops(loop, LoopType.Yoyo)
               .SetEase(_ease)
               .SetAutoKill(true)
               .Play();
    }
}
