using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIInvertColours
{
    public Text _titleText;
    public Image toggleCheckMark;
    public bool invertOnHighlight;
    public bool invertOnSelected;
    public Color invertedHighlightColour = Color.white;
    public Color invertedSelectedColour = Color.white;

    bool _canInvert;
    Color _checkMarkStartColour = Color.white;
    Color _textStartColour = Color.white;

    public Action<UIEventTypes, bool> OnAwake()
    {
        //TODO add error or guide text if both filled in
        _canInvert = CheckSettings();
        if (toggleCheckMark != null) _checkMarkStartColour = toggleCheckMark.color;
        if (_titleText != null) _textStartColour = _titleText.color;
        return InvertColour;
    }

    public Action<UIEventTypes, bool> OnDisable()
    {
        return InvertColour;
    }

    private bool CheckSettings()
    {
        if (_titleText || toggleCheckMark)
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
        if (toggleCheckMark != null) toggleCheckMark.color = newColour;
        if (_titleText != null) _titleText.color = newColour;
    }

    private void StartColour()
    {
        if (toggleCheckMark != null) toggleCheckMark.color = _checkMarkStartColour;
        if (_titleText != null) _titleText.color = _textStartColour;
    }

}
