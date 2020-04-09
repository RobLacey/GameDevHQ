using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonAccessories 
{
    [SerializeField] Image _pointer;
    [SerializeField] GameObject _animation;

    public void OnAwake()
    {
        ActivatePointer(false);
        ActivateAnimation(false);
    }

    public void ActivatePointer(bool active)
    {
        if (_pointer)
        {
            _pointer.enabled = active;
        }
    }

    public void ActivateAnimation(bool active)
    {
        if (_animation)
        {
            _animation.SetActive(active);
        }
    }
}
