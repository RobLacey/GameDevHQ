using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[Serializable]
public class UIAccessories : NodeFunctionBase
{
    [SerializeField] [EnumFlags] AccessoryEventType _activateWhen = AccessoryEventType.None;
    [SerializeField] Image[] _accessoriesList;
    [SerializeField] private Outline[] _outlinesToUse;
    [SerializeField] private Shadow[] _dropShadowsToUse;

    //Variables
    protected override bool CanBeSelected() => (_activateWhen & AccessoryEventType.Selected) != 0;
    protected  override bool CanBeHighlighted() => (_activateWhen & AccessoryEventType.Highlighted) != 0;
    protected override bool CanBePressed() => false;
    protected override bool FunctionNotActive() => !CanActivate || _activateWhen == AccessoryEventType.None;

    public override void OnAwake(UINode node, UiActions uiActions)
    {
        CanActivate = (node.ActiveFunctions & Setting.Accessories) != 0;
        if(FunctionNotActive()) return;
        
        base.OnAwake(node, uiActions);
        StartActivation(false);
    }

    private void StartActivation(bool active)
    {
        ActivateAccessories(active);
        ProcessOutLines(active);
        ProcessShadows(active);
    }

    private void ActivateAccessories(bool active)
    {
        if (_accessoriesList.Length == 0) return;
        foreach (var image in _accessoriesList)
        {
            image.enabled = active;
        }
    }

    private void ProcessOutLines(bool active)
    {
        if (_outlinesToUse.Length == 0) return;
        foreach (var outLine in _outlinesToUse)
        { 
            outLine.enabled = active;
        }
    }

    private void ProcessShadows(bool active)
    {
        if (_dropShadowsToUse.Length == 0) return;
        foreach (var shadow in _dropShadowsToUse)
        {
            shadow.enabled = active;
        }
    }

    private protected override void ProcessSelectedAndHighLighted() => StartActivation(true);

    private protected override void ProcessHighlighted() => StartActivation(true);

    private protected override void ProcessSelected() => StartActivation(true);

    private protected override void ProcessToNormal() => StartActivation(false);
    
    private protected override void ProcessPress()  {  }

    private protected override void ProcessDisabled(bool isDisabled) => StartActivation(false);
}
