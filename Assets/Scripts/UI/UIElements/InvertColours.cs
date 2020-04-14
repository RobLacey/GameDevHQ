﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InvertColours
{
    public Text subText;
    public Image toggleCheckMark;
    public bool invertOnHighlight;
    public bool invertOnSelected;
    public Color invertedColour = Color.white;

    bool canInvert;
    Color standardColour = Color.white;

    public void OnAwake()
    {
        canInvert = CheckSettings();
        if (toggleCheckMark != null) standardColour = toggleCheckMark.color;
        if (subText != null) standardColour = subText.color;
    }

    private bool CheckSettings()
    {
        if (subText || toggleCheckMark)
        {
            if (invertOnSelected || invertOnHighlight)
            {
                return true;
            }
        }
        return false;
    }

    public void InvertColour(UIEventTypes eventType)
    {
        if (canInvert)
        {
            switch (eventType)
            {
                case UIEventTypes.Normal:
                    ChangeColour(standardColour);
                    break;
                case UIEventTypes.Highlighted:
                    if (invertOnHighlight)
                    {
                        ChangeColour(invertedColour);
                    }
                    break;
                case UIEventTypes.Selected:
                    if (invertOnSelected)
                    {
                        ChangeColour(invertedColour);
                    }
                    break;
                case UIEventTypes.Cancelled:
                    ChangeColour(standardColour);
                    break;
                default:
                    break;
            }
        }    
    }

    private void ChangeColour(Color newColour)
    {
        if (toggleCheckMark != null) toggleCheckMark.color = newColour;
        if (subText != null) subText.color = newColour;
    }

}
