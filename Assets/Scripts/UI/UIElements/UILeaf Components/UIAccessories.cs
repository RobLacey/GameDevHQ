using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[Serializable]
public class UIAccessories
{
    [SerializeField] [EnumFlags] AccessoryEventType _activateWhen = AccessoryEventType.None;
    [SerializeField] [AllowNesting] [EnableIf("Activate")] Outline _useOutline;
    [SerializeField] [AllowNesting] [EnableIf("Activate")] Shadow _useShadow;
    [SerializeField] Image[] _accessoriesList;

    //Variables
    Setting _mySettings = Setting.Accessories;

    //Editor Script
    public bool Activate()
    {
        if ((_activateWhen == AccessoryEventType.None))
        {
            return false;
        }
        return true;
    }

    public Action<UIEventTypes, bool, Setting> OnAwake()
    {
        ActivatePointer(UIEventTypes.Normal, false, _mySettings);
        return ActivatePointer;
    }

    public Action<UIEventTypes, bool, Setting> OnDisable()
    {
        return ActivatePointer;
    }

    private void ActivatePointer(UIEventTypes uIEventTypes, bool active, Setting setting)
    {
        if (!((setting & _mySettings) != 0)) { ActivateAccessories(false); return; }

        switch (uIEventTypes)
        {
            case UIEventTypes.Normal:
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

        if (_accessoriesList.Length > 0)
        {
            foreach (var item in _accessoriesList)
            {
                item.enabled = active;
            }
        }
    }
}
