using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class UISize
{
    [SerializeField] Choose _ChangeSizeOn = Choose.None;
    [SerializeField] [AllowNesting] [ShowIf("Activate")] ScaleType _scaledType = ScaleType.ScaleDown;
    [SerializeField] [AllowNesting] [ShowIf("Activate")] [Range(0.01f, 0.3f)] float _scaleChangeBy = 0.05f;
    [SerializeField] [AllowNesting] [ShowIf("Activate")] [Range(0f, 0.3f)] float _pressedHoldTime = 0.05f;

    //Variables
    Transform _myTransform;
    Vector3 _startSize;
    enum ScaleType { ScaleUp, ScaleDown }

    enum Choose { None, Highlighted, HighlightedAndSelected, Selected, Pressed };

    public bool Activate()
    { 
        if (_ChangeSizeOn != Choose.None)
        {
            return true;
        }
        return false;
    }

    public Action<UIEventTypes, bool> OnAwake(Transform newTransform)
    {
        _myTransform = newTransform;
        _startSize = newTransform.localScale;
        return ProcessChangeOfSize;
    }

    public Action<UIEventTypes, bool> OnDisable()
    {
        return ProcessChangeOfSize;
    }

    private void ProcessChangeOfSize(UIEventTypes uIEventTypes, bool active)
    {
        if (_ChangeSizeOn == Choose.Pressed) return;

        else if (_ChangeSizeOn == Choose.HighlightedAndSelected)
        {
            ProcessHighlighedAndSelected(uIEventTypes, active);
        }

        else if (uIEventTypes == UIEventTypes.Highlighted && _ChangeSizeOn == Choose.Highlighted)
        {
            SetScaleProcess();
        }


        else if (uIEventTypes == UIEventTypes.Normal)
        {
            _myTransform.localScale = _startSize;
        }

        else if (uIEventTypes == UIEventTypes.Selected && _ChangeSizeOn == Choose.Selected)
        {
            ProcessSelected(active);
        }

        if (_ChangeSizeOn == Choose.Highlighted && active) // Fixes scale not returning to normal when slected with this type
        {
            _myTransform.localScale = _startSize;
        }
    }

    private void ProcessSelected(bool active)
    {
        if (active)
        {
            SetScaleProcess();
        }
        else
        {
            if (_scaledType == ScaleType.ScaleUp)
            {
                ScaleDown();
            }
            else
            {
                ScaleUp();
            }
        }
    }

    private void ProcessHighlighedAndSelected(UIEventTypes uIEventTypes, bool active)
    {
        if (active)
        {
            if (uIEventTypes == UIEventTypes.Highlighted)
            {
                SetScaleProcess();
            }

            if (uIEventTypes == UIEventTypes.Selected)
            {
                _myTransform.localScale = _startSize;
            }
        }
        else
        {
            if (uIEventTypes == UIEventTypes.Highlighted)
            {
                SetScaleProcess();
            }

            if (uIEventTypes == UIEventTypes.Normal)
            {
                _myTransform.localScale = _startSize;
            }

        }
    }

    private void SetScaleProcess()
    {
        if (_scaledType == ScaleType.ScaleUp)
        {
            ScaleUp();
        }
        else
        {
            ScaleDown();
        }
    }

    private void ScaleUp()
    {
        float temp = _startSize.x + _scaleChangeBy;

        if (_myTransform.localScale.x >= temp) return;
        _myTransform.localScale += new Vector3(_scaleChangeBy, _scaleChangeBy, 0);
    }

    private void ScaleDown()
    {
        float temp = _startSize.x - _scaleChangeBy;

        if (_myTransform.localScale.x <= temp) return;
        _myTransform.localScale -= new Vector3(_scaleChangeBy, _scaleChangeBy, 0);
    }

    public IEnumerator PressedSequence()
    {
        if (_ChangeSizeOn == Choose.Pressed)
        {
            Vector3 difference = _myTransform.localScale - _startSize; ;
            float scaleBy = _scaleChangeBy;

            if (_scaledType == ScaleType.ScaleDown)
            {
                scaleBy = _scaleChangeBy * -1;
            }

            _myTransform.localScale += new Vector3(scaleBy, scaleBy, 0);
            yield return new WaitForSeconds(_pressedHoldTime);
            _myTransform.localScale = _startSize + difference;
        }
        else { yield return null; }
    }
}
