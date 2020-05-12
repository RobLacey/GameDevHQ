using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[System.Serializable]
public class UIInvertColours
{
    [InfoBox("CANNOT have Text AND Image set.", EInfoBoxType.Warning)]
    [SerializeField]  bool invertOnHighlight;
    [SerializeField] bool invertOnSelected;
    [SerializeField] [AllowNesting] [ShowIf(EConditionOperator.Or, "invertOnHighlight", "invertOnSelected")] [DisableIf("ImageSet")]
    Text _text;
    [SerializeField] [AllowNesting] [ShowIf(EConditionOperator.Or, "invertOnHighlight", "invertOnSelected")] [DisableIf("TextSet")]
    Image _image;
    [SerializeField] [AllowNesting] [ShowIf("invertOnHighlight")] Color invertedHighlightColour = Color.white;
    [SerializeField] [AllowNesting] [ShowIf("invertOnSelected")] Color invertedSelectedColour = Color.white;

    bool _canInvert;
    Color _checkMarkStartColour = Color.white;
    Color _textStartColour = Color.white;

    #region Editor Scripts
    private bool TextSet()
    {
        if (_text != null) { return true; }
        return false;
    }
    private bool ImageSet()
    {
        if (_image != null) { return true; }
        return false;
    }

    #endregion
    public Action<UIEventTypes, bool> OnAwake()
    {
        _canInvert = CheckSettings();
        if (_image != null) _checkMarkStartColour = _image.color;
        if (_text != null) _textStartColour = _text.color;
        return InvertColour;
    }

    public Action<UIEventTypes, bool> OnDisable()
    {
        return InvertColour;
    }

    private bool CheckSettings()
    {
        if (_text || _image)
        {
            return true;
        }
        return false;
    }

    public void InvertColour(UIEventTypes eventType, bool selected)
    {
        if (_canInvert)
        {
            switch (eventType)
            {
                case UIEventTypes.Normal:
                    StartColour();
                    break;
                case UIEventTypes.Highlighted:
                    if (invertOnHighlight)
                    {
                        ChangeColour(invertedHighlightColour);
                    }
                    else
                    {
                        StartColour();
                    }
                    break;
                case UIEventTypes.Selected:
                    if (invertOnSelected)
                    {
                        if (selected)
                        {
                            ChangeColour(invertedSelectedColour);
                        }
                        else
                        {
                            InvertColour(UIEventTypes.Highlighted, selected);
                        }
                    }
                    else
                    {
                        StartColour();
                    }
                    break;
                default:
                    break;
            }
        }    
    }

    private void ChangeColour(Color newColour)
    {
        if (_image != null) _image.color = newColour;
        if (_text != null) _text.color = newColour;
    }

    private void StartColour()
    {
        if (_image != null) _image.color = _checkMarkStartColour;
        if (_text != null) _text.color = _textStartColour;
    }

}
