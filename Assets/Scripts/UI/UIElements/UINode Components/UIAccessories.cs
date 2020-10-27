using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[Serializable]
public class UIAccessories : NodeFunctionBase
{
    [SerializeField] [EnumFlags] private AccessoryEventType _activateWhen = AccessoryEventType.None;
    [SerializeField] private Image[] _accessoriesList;
    [SerializeField] private Outline[] _outlinesToUse;
    [SerializeField] private Shadow[] _dropShadowsToUse;

    //Variables
    protected override bool CanBeHighlighted() => (_activateWhen & AccessoryEventType.Highlighted) != 0;
    protected override bool CanBePressed() => (_activateWhen & AccessoryEventType.Selected) != 0;
    protected override bool FunctionNotActive() => !CanActivate || _activateWhen == AccessoryEventType.None;

    public override void OnAwake(UiActions uiActions, Setting activeFunctions)
    {
        base.OnAwake(uiActions, activeFunctions);
        CanActivate = (_enabledFunctions & Setting.Accessories) != 0;
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
        StartActivation(_isSelected);
    }

    private protected override void ProcessDisabled()
    {
        if(!CanActivate) return;
        StartActivation(false);
    }
}
