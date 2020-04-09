using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonColour
{
    public Image[] _images;
    public Text _mainText;
    public Color _selectedColour = Color.white;
    public Color _highlightedColour = Color.white;
    public Color _normalColour = Color.white;
    public Color _pressedColour = Color.white;
    public bool _highlight;
    public bool _pressed;
    public bool _selected;
    public InvertText _invertTextColour;
    bool _canInverseTextColour;
    public ColourLerp _colourLerp;

    [System.Serializable]
    public class InvertText
    {
        public Text subText;
        public bool invertOnHighlight;
        public bool invertOnPressed;
        public bool invertOnSelected;
        public Color standardColour = Color.white;
        public Color inverseColour = Color.white;

        public bool Inverted { get; set; }

        public bool CheckSettings()
        {
            if (invertOnPressed || invertOnSelected || invertOnHighlight)
            {
                return true;
            }
            return false;
        }

        public void InvertTextColours()
        {
            if (subText)
            {
                if (!Inverted)
                {
                    subText.color = standardColour;
                    Inverted = true;
                }
                else
                {
                    subText.color = inverseColour;
                    Inverted = false;
                }

            }        
        }
    }

    public void OnAwake()
    {
        if (_invertTextColour.CheckSettings())
        {
            _canInverseTextColour = true;
        }
        if (_images.Length > 0) _normalColour = _images[0].color;
        if (_mainText) _normalColour = _mainText.color;
    }

    public void SetColour(UIEventTypes uIEventTypes)
    {
        if (_images.Length > 0 || _mainText || _invertTextColour.subText)
        {
            switch (uIEventTypes)
            {
                case UIEventTypes.Normal:
                    SetUIColour(_normalColour);
                    _invertTextColour.Inverted = false;
                    break;
                case UIEventTypes.Highlighted:
                    if (_highlight)
                    {
                        _invertTextColour.Inverted = _invertTextColour.invertOnHighlight;
                        SetUIColour(_highlightedColour);
                    }
                    break;
                case UIEventTypes.Selected:
                    if (_selected)
                    {
                        _invertTextColour.Inverted = _invertTextColour.invertOnSelected;
                        SetUIColour(_selectedColour);
                    }
                    break;
                case UIEventTypes.Pressed:
                    if (_pressed)
                    {
                        _invertTextColour.Inverted = _invertTextColour.invertOnPressed;
                        SetUIColour(_pressedColour);
                    }
                    break;
                default:
                    break;
            }

        }
    }

    private void SetUIColour(Color newColour)
    {
        if (_images.Length > 0)
        {
            foreach (var item in _images)
            {
                item.color = newColour;
            }
        }

        if (_mainText && !_canInverseTextColour)
        {
            if (_colourLerp != null)
            {
                _colourLerp.StartLerp(_mainText.color, newColour, (x) => SetTextColour(x));
            }
            else
            {
                _mainText.color = newColour;
            }
        }

        if (_canInverseTextColour)
        {
            _invertTextColour.InvertTextColours();
        }
    }

    private void SetTextColour(Color newColour)
    {
        _mainText.color = newColour;
    }

}
