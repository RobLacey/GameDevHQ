﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class UIAccessories : NodeFunctionBase
{
    public UIAccessories(IAccessoriesSettings settings, IUiEvents uiEvents) : base(uiEvents)
    {
        _activateWhen = settings.ActivateWhen;
        _accessoriesList = settings.AccessoriesList;
        _outlinesToUse = settings.OutLineList;
        _dropShadowsToUse = settings.ShadowList;
        CanActivate = true;
    }

    //Variables
    private readonly AccessoryEventType _activateWhen;
    private readonly Image[] _accessoriesList;
    private readonly Outline[] _outlinesToUse;
    private readonly Shadow[] _dropShadowsToUse;

    //Properties
    protected override bool CanBeHighlighted() => (_activateWhen & AccessoryEventType.Highlighted) != 0;
    protected override bool CanBePressed() => (_activateWhen & AccessoryEventType.Selected) != 0;
    protected override bool FunctionNotActive() => !CanActivate || _activateWhen == AccessoryEventType.None;

    public override void OnAwake()
    {
        base.OnAwake();
        StartActivation(false);
    }

    private void StartActivation(bool active)
    {
        ProcessEffect(_accessoriesList, active);
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

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive() || !CanBeHighlighted() || CanBePressed() && _isSelected) return;
        _pointerOver = pointerOver;
        StartActivation(pointerOver);
    }

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

    private protected override void ProcessDisabled()
    {
        if(!CanActivate) return;
        StartActivation(false);
    }
}
