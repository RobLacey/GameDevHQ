using System;
using UnityEngine;

public interface ISizeAndPosition : IPositionScaleTween, IPunchShakeTween { }

public class UISizeAndPosition : NodeFunctionBase, ISizeAndPosition, IAlwaysHighlightSettings
{
    public UISizeAndPosition(ISizeAndPositionSettings settings, IUiEvents uiEvents): base(uiEvents)
    {
        _settings = settings;
        Scheme = settings.Scheme;
        MyBranch = settings.ParentBranch;
        if(settings.RectTransform != null)
            MyRect = settings.RectTransform;
    }

    //Variables
    private INodeTween _tween;
    private readonly ISizeAndPositionSettings _settings;
    private IAlwaysHighlight _alwaysHighlighted;

    //Properties & Set/Getters
    public SizeAndPositionScheme Scheme { get; }
    public bool IsPressed { get; private set; }
    public RectTransform MyRect { get; }
    public Transform MyTransform { get; private set; }
    public Vector3 StartPosition { get; private set; }
    public Vector3 StartSize { get; private set; }
    public string GameObjectID { get; private set; }
    public INode UiNode => _uiEvents.ReturnMasterNode;
    public bool IsSelected => _isSelected;
    public Action DoPointerOverSetUp => PointerOver;
    public Action DoPointerNotOver => PointerNotOver;
    protected override bool CanBeHighlighted() => Scheme.CanBeHighlighted || Scheme.CanBeSelectedAndHighlight;
    protected override bool CanBePressed()  => !Scheme.NotSet && !Scheme.CanBeSelectedAndHighlight;
    public IBranch MyBranch { get; }
    public Override Overridden => _settings.OverrideAlwaysHighlighted;
    public override bool FunctionNotActive() => _isDisabled || Scheme.NotSet;
    public bool OptionalStartConditions => !CanBeHighlighted();

    //Main
    public override void OnAwake()
    {
        if(!Scheme || MyRect is null) return;
        base.OnAwake();
        Scheme.OnAwake();
        GameObjectID = _uiEvents.MasterNodeID.ToString();
        SetVariables();
        if(MyBranch.AlwaysHighlighted == IsActive.Yes)
            _alwaysHighlighted = EZInject.Class.WithParams<IAlwaysHighlight>(this);
    }
    
    public override void OnEnable()
    {
        base.OnEnable();
        if(_alwaysHighlighted.IsNotNull())
            _alwaysHighlighted.ObserveEvents();
    }
    
    protected override void LateStartSetUp()
    {
        base.LateStartSetUp();
        if(MyHubDataIsNull) return;
        
        if (_myDataHub.SceneStarted)
        {
            _alwaysHighlighted.ShowStartingHighlightedNode();
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        IsPressed = false;
        if(_alwaysHighlighted.IsNotNull())
            _alwaysHighlighted.UnObserveEvents();
    }

    private void SetVariables()
    {
        MyTransform = MyRect.transform;
        StartSize = MyRect.localScale;
        StartPosition = MyRect.anchoredPosition3D;
        _tween = SizeAndPositionFactory.AssignType(Scheme.TweenEffect, this);
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
            PointerOver();
        }
        else
        {
            if (_alwaysHighlighted.IsNotNull() && _alwaysHighlighted.CanAllow()) return;
            PointerNotOver();
        }    
    }

    private void PointerOver()
    {
        if (_isSelected && Scheme.CanBeSelectedAndHighlight || _pointerOver) return;
        _tween.DoTween(IsActive.Yes);
        _pointerOver = true;
    }

    private void PointerNotOver()
    {
        if (_isSelected && Scheme.CanBeSelectedAndHighlight || !_pointerOver) return;
        _tween.DoTween(IsActive.No);
        _pointerOver = false;
    }

    private protected override void ProcessPress()
    { 
        if(FunctionNotActive() || !CanBePressed()) return;
        
        if(Scheme.IsPressed)
        {
            IsPressed = true;
            _tween.DoTween(IsActive.Yes);
            IsPressed = false;
        }
        else if(Scheme.CanBeSelected || Scheme.CanBeSelectedAndHighlight)
        {
            if(_pointerOver) return;
            _tween.DoTween(_isSelected ? IsActive.Yes : IsActive.No);
        }
    }

    private protected override void ProcessDisabled() => _tween.DoTween(IsActive.No);
}
