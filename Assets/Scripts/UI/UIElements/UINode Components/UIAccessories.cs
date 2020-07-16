using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEngine.EventSystems;

[Serializable]
public class UIAccessories
{
    [SerializeField] [EnumFlags] AccessoryEventType _activateWhen = AccessoryEventType.None;
    [SerializeField] [AllowNesting] [EnableIf("Activate")] Outline _useOutline;
    [SerializeField] [AllowNesting] [EnableIf("Activate")] Shadow _useShadow;
    [SerializeField] Image[] _accessoriesList;

    //Variables
    bool _canActivate;

    //Editor Script
    public bool Activate()
    {
        if ((_activateWhen == AccessoryEventType.None))
        {
            return false;
        }
        return true;
    }

    public Action<UIEventTypes, bool> OnAwake(Setting setting)
    {
        _canActivate = (setting & Setting.Accessories) != 0;
        ActivatePointer(UIEventTypes.Normal, false);
        return ActivatePointer;
    }

    public Action<UIEventTypes, bool> OnDisable()
    {
        return ActivatePointer;
    }

    private void ActivatePointer(UIEventTypes uIEventTypes, bool active)
    {
        if (!_canActivate) { ActivateAccessories(false); return; }

        switch (uIEventTypes)
        {
            case UIEventTypes.Normal:
                //ebug.Log(EventSystem.cur.isFocused()rent);
                ActivateAccessories(false);
                break;
            case UIEventTypes.Highlighted:
                if ((_activateWhen & AccessoryEventType.Highlighted) != 0)
                {
                    ActivateAccessories(true);
                }
                break;
            case UIEventTypes.Selected:
                if ((_activateWhen & AccessoryEventType.Selected) != 0)
                {
                    ActivateAccessories(true);
                }
                break;
            case UIEventTypes.Cancelled:
                ActivateAccessories(false);
                break;
        }
    }

    private void ActivateAccessories(bool active)
    {
        if(_useOutline) _useOutline.enabled = active;
        if(_useShadow) _useShadow.enabled = active;
        if (_accessoriesList.Length <= 0) return;
        
        foreach (var item in _accessoriesList)
        {
            item.enabled = active;
        }
    }
}
