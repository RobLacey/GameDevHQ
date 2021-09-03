using System;
using UnityEngine;
using UnityEngine.UI;

public class UIAccessories : NodeFunctionBase, IAlwaysHighlightSettings
{
    public UIAccessories(IAccessoriesSettings settings, IUiEvents uiEvents) : base(uiEvents)
    {
        _settings = settings;
        _activateWhen = settings.ActivateWhen;
        MyBranch = settings.ParentBranch;
        _accessoriesList = settings.AccessoriesList;
        _outlinesToUse = settings.OutLineList;
        _dropShadowsToUse = settings.ShadowList;
    }

    //Variables
    private readonly AccessoryEventType _activateWhen;
    private readonly GameObject[] _accessoriesList;
    private readonly Outline[] _outlinesToUse;
    private readonly Shadow[] _dropShadowsToUse;
    private readonly IAccessoriesSettings _settings;
    private IAlwaysHighlight _alwaysHighlighted;

    //Properties
    protected override bool CanBeHighlighted() => (_activateWhen & AccessoryEventType.Highlighted) != 0;
    protected override bool CanBePressed() => (_activateWhen & AccessoryEventType.Selected) != 0;
    public IBranch MyBranch { get; }
    public Override Overridden => _settings.OverrideAlwaysHighlighted;
    public override bool FunctionNotActive() => _isDisabled || _activateWhen == AccessoryEventType.None;
    public INode UiNode => _uiEvents.ReturnMasterNode;
    public bool IsSelected => _isSelected;
    public Action DoPointerOverSetUp => PointerOver;
    public Action DoPointerNotOver => PointerNotOver;
    public bool OptionalStartConditions => !CanBeHighlighted();


    public override void OnAwake()
    {
        base.OnAwake();
        StartActivation(false);
        if(MyBranch.AlwaysHighlighted == IsActive.Yes)
            _alwaysHighlighted = EZInject.Class.WithParams<IAlwaysHighlight>(this);
    }
    
    public override void OnEnable()
    {
        base.OnEnable();
        if(_alwaysHighlighted.IsNotNull())
            _alwaysHighlighted.ObserveEvents();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if(_alwaysHighlighted.IsNotNull())
            _alwaysHighlighted.UnObserveEvents();
    }

    private void StartActivation(bool active)
    {
        ProcessEffectGameObject(_accessoriesList, active);
        ProcessEffect(_outlinesToUse, active);
        ProcessEffect(_dropShadowsToUse, active);
    }

    private void ProcessEffect(Array array, bool active)
    {
        if (array.Length == 0) return;
        
        foreach (Behaviour item in array)
        {
            item.enabled = active;
        }
    }
    
    private void ProcessEffectGameObject(Array array, bool active)
    {
        if (array.Length == 0) return;
        
        foreach (GameObject item in array)
        {
            item.SetActive(active);
        }
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive() || !CanBeHighlighted() || CanBePressed() && _isSelected) return;
        
        _pointerOver = pointerOver;
        
        if (pointerOver)
        {
            StartActivation(_pointerOver);
        }
        else
        {  
            if(_alwaysHighlighted.IsNotNull() && _alwaysHighlighted.CanAllow()) return;
            PointerNotOver();
        }
        
    }

    private void PointerOver() => StartActivation(true);

    private void PointerNotOver() => StartActivation(_pointerOver);

    protected override void SaveIsSelected(bool isSelected)
    {
        base.SaveIsSelected(isSelected);
        ProcessPress();
    }

    private protected override void ProcessPress()
    {
        if(FunctionNotActive() || !CanBePressed()) return;
        if(CanBeHighlighted() && _pointerOver) return;
        StartActivation(_isSelected);
    }

    private protected override void ProcessDisabled() => StartActivation(false);
}
