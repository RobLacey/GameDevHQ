using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public interface IAccessoriesSettings : IComponentSettings
{
    AccessoryEventType ActivateWhen { get; }
    Image[] AccessoriesList { get; }
    Outline[] OutLineList { get; }
    Shadow[] ShadowList { get; }
}

[Serializable]
public class AccessoriesSettings : IAccessoriesSettings
{
    [SerializeField] [EnumFlags] private AccessoryEventType _activateWhen = AccessoryEventType.None;
    [SerializeField] private Image[] _accessoriesList;
    [SerializeField] private Outline[] _outlinesToUse;
    [SerializeField] private Shadow[] _dropShadowsToUse;

    public AccessoryEventType ActivateWhen => _activateWhen;
    public Image[] AccessoriesList => _accessoriesList;
    public Outline[] OutLineList => _outlinesToUse;
    public Shadow[] ShadowList => _dropShadowsToUse;
    
    public NodeFunctionBase SetUp(UiActions uiActions, Setting functions)
    {
        if ((functions & Setting.Accessories) != 0)
        {
            return new UIAccessories(this, uiActions);
        }
        return new NullFunction();
    }

}