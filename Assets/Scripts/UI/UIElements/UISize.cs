using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UISize
{
    [SerializeField] Choose _ChangeSizeOn = Choose.None;
    [SerializeField] ScaleType _scaledType = ScaleType.ScaleDown;
    [SerializeField] [Range(0.01f, 0.3f)] float _scaleChangeBy = 0.05f;
    [SerializeField] [Range(0f, 0.3f)] float _pressedHoldTime = 0.05f;

    //Variables
    Transform _myTransform;
    Vector3 _startSize;
    enum ScaleType { ScaleUp, ScaleDown }

    enum Choose { None, Highlighted, Selected, Pressed };

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

        if (uIEventTypes == UIEventTypes.Highlighted)
        {
            if (_ChangeSizeOn == Choose.Highlighted)
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
        }

        if (uIEventTypes == UIEventTypes.Normal)
        {
            _myTransform.localScale = _startSize;
        }

        if (uIEventTypes == UIEventTypes.Selected)
        {
            if (_ChangeSizeOn == Choose.Selected)
            {
                if (active)
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
