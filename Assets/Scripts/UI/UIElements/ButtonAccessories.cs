using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonAccessories 
{
    [SerializeField] Image[] _activationList;

    public void OnAwake()
    {
        ActivatePointer(false);
    }

    public void ActivatePointer(bool active)
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
