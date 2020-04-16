using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UISize
{
    [SerializeField] Choose _ChangeSizeOn;
    [SerializeField] [Range(0.01f, 0.1f)] float _highlightedSize = 0.05f;
    [SerializeField] bool _onPressed;
    [SerializeField] [Range(-0.1f, 0.1f)] float _pressedSize = 0.05f;
    [SerializeField] [Range(0f, 0.3f)] float _pressedTime = 0.05f;

    //Variables
    Transform _myTransform;
    Vector3 _startSize;
    enum Choose { None, Highlighted, Selected };

    public void OnAwake(Transform newTransform)
    {
        _myTransform = newTransform;
        _startSize = newTransform.localScale;
    }

    public void HighlightedScaleUp()
    {
        if (_ChangeSizeOn == Choose.Highlighted)
        {
            ChangeSize();
        }
    }

    public void SelectedScaleUp()
    {
        if (_ChangeSizeOn == Choose.Selected)
        {
            ChangeSize();
        }
    }


    public void HighlightedScaleDown()
    {
        if (_ChangeSizeOn == Choose.Highlighted)
        {
            _myTransform.localScale = _startSize;
        }
    }

    public void SelectedScaleDown()
    {
        if (_ChangeSizeOn == Choose.Selected)
        {
            _myTransform.localScale = _startSize;
        }
    }

    private void ChangeSize()
    {
        float temp = _startSize.x + _highlightedSize;
        if (_myTransform.localScale.x < temp)
        {
            _myTransform.localScale += new Vector3(_highlightedSize, _highlightedSize, 0);
        }
    }

    public IEnumerator PressedSequence()
    {
        if (_onPressed)
        {
            Vector3 difference = _myTransform.localScale - _startSize;
            _myTransform.localScale -= new Vector3(-_pressedSize, -_pressedSize, 0);
            yield return new WaitForSeconds(_pressedTime);
            _myTransform.localScale = _startSize + difference;
        }
        else
            { yield return null; }
    }
}
