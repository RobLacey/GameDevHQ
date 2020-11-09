using UnityEngine;

public class UISizeAndPosition : NodeFunctionBase, IPositionScaleTween, IPunchShakeTween
{
    public UISizeAndPosition(ISizeAndPositionSettings settings, IUiEvents uiEvents)
    {
        _scheme = settings.Scheme;
        CanActivate = true;
        if(settings.RectTransform != null)
            MyRect = settings.RectTransform;
        OnAwake(uiEvents);
    }

    //Variables
    private readonly SizeAndPositionScheme _scheme;
    private INodeTween _tween;
    private string _gameObjectID;

    //Properties
    public SizeAndPositionScheme Scheme => _scheme;
    public bool IsPressed { get; private set; }
    public RectTransform MyRect { get; }
    public Transform MyTransform { get; private set; }
    public Vector3 StartPosition { get; private set; }
    public Vector3 StartSize { get; private set; }
    protected override bool CanBeHighlighted() => _scheme.CanBeHighlighted || _scheme.CanBeSelectedAndHighlight;
    protected override bool CanBePressed()  => !_scheme.NotSet && !_scheme.CanBeSelectedAndHighlight;
    protected override bool FunctionNotActive() => !CanActivate && !_scheme.NotSet;

    //Main
    protected sealed override void OnAwake(IUiEvents events)
    {
        if(!_scheme || MyRect is null) return;
        base.OnAwake(events);
        _scheme.OnAwake();
        _gameObjectID = events.MasterNodeID.ToString();
        SetVariables();
    }

    private void SetVariables()
    {
        MyTransform = MyRect.transform;
        StartSize = MyRect.localScale;
        StartPosition = MyRect.anchoredPosition3D;
        if(CanActivate)
            _tween = SizeAndPositionFactory.AssignType(_scheme.TweenEffect, this, _gameObjectID);
    }

    protected override void SaveIsSelected(bool isSelected)
    {
        base.SaveIsSelected(isSelected);
        ProcessPress();
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive()|| !CanBeHighlighted()) return;
        
        if(pointerOver)
        {
            if (_isSelected && _scheme.CanBeSelectedAndHighlight || _pointerOver) return;
            _tween.DoTween(IsActive.Yes);
            _pointerOver = true;
        }
        else
        {

            if (_isSelected && _scheme.CanBeSelectedAndHighlight || !_pointerOver) return;
            _tween.DoTween(IsActive.No);
            _pointerOver = false;
        }    
    }

    private protected override void ProcessPress()
    { 
        if(FunctionNotActive() || !CanBePressed()) return;
        
        if(_scheme.IsPressed)
        {
            IsPressed = true;
            _tween.DoTween(IsActive.Yes);
            IsPressed = false;
        }
        else if(_scheme.CanBeSelected || _scheme.CanBeSelectedAndHighlight)
        {
            if(_pointerOver) return;
            _tween.DoTween(_isSelected ? IsActive.Yes : IsActive.No);
        }
    }

    private protected override void ProcessDisabled()
    {
        if(!CanActivate) return;
        _tween.DoTween(IsActive.No);
    }
}
