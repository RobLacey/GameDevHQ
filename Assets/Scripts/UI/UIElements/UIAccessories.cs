using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIAccessories 
{
    [SerializeField] Image[] _activationList;

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
                ActivateAccessories(true);
                break;
            case UIEventTypes.Selected:
                break;
            case UIEventTypes.Cancelled:
                break;
            default:
                break;
        }
    }

    private void ActivateAccessories(bool active)
    {
        if (_activationList.Length > 0)
        {
            foreach (var item in _activationList)
            {
                item.enabled = active;
            }
        }
    }
}
