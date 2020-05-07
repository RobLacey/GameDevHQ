using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;


[System.Serializable]
public class UIAccessories 
{
    [SerializeField] EventType _activateWhen = EventType.Never;
    [SerializeField] [AllowNesting] [EnableIf("Activate")] Outline _useOutline;
    [SerializeField] [AllowNesting] [EnableIf("Activate")] Shadow _useShadow;
    [SerializeField] Image[] _accessoriesList;

    enum EventType { Never, Highlighted, Selected }

    public bool Activate() { if (_activateWhen != EventType.Never) return true; return false; } 

    public Action<UIEventTypes, bool> OnAwake()
    {
        ActivatePointer(UIEventTypes.Normal, false);
        return ActivatePointer;
    }

    public Action<UIEventTypes, bool> OnDisable()
    {
        return ActivatePointer;
    }


    public void ActivatePointer(UIEventTypes uIEventTypes, bool active)
    {
        switch (uIEventTypes)
        {
            case UIEventTypes.Normal:
                ActivateAccessories(false);
                break;
            case UIEventTypes.Highlighted:
                if (_activateWhen == EventType.Highlighted)
                {
                    ActivateAccessories(true);
                }
                break;
            case UIEventTypes.Selected:
                if (_activateWhen == EventType.Selected)
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
