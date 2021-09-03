using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public interface IAccessoriesSettings : IComponentSettings, IOverride
{
    AccessoryEventType ActivateWhen { get; }
    GameObject[] AccessoriesList { get; }
    Outline[] OutLineList { get; }
    Shadow[] ShadowList { get; }
    IBranch ParentBranch { get; }
}

[Serializable]
public class AccessoriesSettings : IAccessoriesSettings
{
    [SerializeField] [EnumFlags] 
    private AccessoryEventType _activateWhen = AccessoryEventType.None;

    [SerializeField] 
    private Override _overrideAlwaysHighlighted = Override.Allow;

    [FormerlySerializedAs("_accessoriesListTest")] [SerializeField] [Space(10f)]
    private GameObject[] _accessoriesList;

    [SerializeField] 
    private Outline[] _outlinesToUse;

    [SerializeField] 
    private Shadow[] _dropShadowsToUse;


    //Properties, Setters & Getters
    public AccessoryEventType ActivateWhen => _activateWhen;
    public GameObject[] AccessoriesList => _accessoriesList;
    public Outline[] OutLineList => _outlinesToUse;
    public Shadow[] ShadowList => _dropShadowsToUse;
    public IBranch ParentBranch { get; private set; }
    public Override OverrideAlwaysHighlighted => _overrideAlwaysHighlighted;


    //Main
    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        ParentBranch = uiNodeEvents.ReturnMasterNode.MyBranch;
        CheckForSetUpError(functions, uiNodeEvents.ReturnMasterNode);
        
        if (CanCreate(functions))
        {
            return new UIAccessories(this, uiNodeEvents);
        }
        return null;
    }
    
    private bool CanCreate(Setting functions) => (functions & Setting.Accessories) != 0;
    
    private void CheckForSetUpError(Setting functions, UINode parentNode) 
    {
        if(!CanCreate(functions)) return;
        if (_accessoriesList.Length > 0 && _accessoriesList[0] is null)
        {
            throw new Exception($"No accessory image set in Accessories settings for {parentNode}");
        }
        if (_outlinesToUse.Length > 0 && _outlinesToUse[0] is null)
        {
            throw new Exception($"No outline image set in Accessories settings for {parentNode}");
        }
        if (_dropShadowsToUse.Length > 0 && _dropShadowsToUse[0] is null)
        {
            throw new Exception($"No drop shadow image set in Accessories settings for {parentNode}");
        }
        
    }
}