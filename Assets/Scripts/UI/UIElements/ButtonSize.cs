using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ButtonSize
{
    public bool _changeSize;
    [SerializeField] [Range(-0.3f, 0.3f)] public float _sizeChange;

    //Variables
    Vector3 _startSize;

    public void OnAwake(Transform newTransform)
    {
        _startSize = newTransform.localScale;
    }

    public void ScaleUp(Transform newTransform)
    {
        if (_changeSize)
        {
            if(newTransform.localScale.x == _startSize.x)
            {
                newTransform.localScale += new Vector3(_sizeChange, 0, 0);
            }
        }
    }

    public void ScaleDown(Transform newTransform)
    {
        if (_changeSize)
        {
            if (newTransform.localScale.x != _startSize.x)
            {
                newTransform.localScale = _startSize;
            }
        }
    }
}
