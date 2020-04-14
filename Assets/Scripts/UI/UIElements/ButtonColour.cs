using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonColour
{
    public Image[] _images;
    public Text _mainText;
    public bool _LerpImageColours;
    public bool _LerpTextColours;
    public float _crossFadeTime = 0.4f;
    public Color _selectedColour = Color.white;
    public Color _highlightedColour = Color.white;
    public bool _highlight;
    public bool _selected;
    public bool _pressFlash;
    public Color _flashColour = Color.white;

    //Variables
    public bool PressFlash { set { _pressFlash = value; } }
    public bool Selected { set { _selected = value; } }
    public ColourLerp MyColourLerper { get; set; }
    Color _normalColour = Color.white;
    float _flashTimer = 0.1f;

    public void OnAwake()
    {
        if (_images.Length > 0) _normalColour = _images[0].color;
        if (_mainText) _normalColour = _mainText.color;
        if(MyColourLerper != null) MyColourLerper.StartColour = _normalColour;
    }

    public void OnSelectedColourChange(UIEventTypes uIEventTypes)
    {
        if (_pressFlash)
        {
            MyColourLerper.NewLerp();
            if (_images.Length > 0)
            {
                MyColourLerper.StartFlash(_flashColour, _flashTimer, (x) => SetImageColour(x), GetColour(uIEventTypes));
            }

            if (_mainText)
            {
                MyColourLerper.StartFlash(_flashColour, _flashTimer, (x) => SetTextColour(x), GetColour(uIEventTypes));
            }
        }
        else
        {
            SetUIColour(uIEventTypes);
        }
    }

    private Color GetColour(UIEventTypes uIEventTypes)
    {
        switch (uIEventTypes)
        {
            case UIEventTypes.Normal:
                return _normalColour;
            case UIEventTypes.Highlighted:
                if (_highlight)
                    { return _highlightedColour;}
                else
                    { return _normalColour; }
            case UIEventTypes.Selected:
                if (_selected)
                    { return _selectedColour; }
                else
                    { return _normalColour; }
        }
        return _normalColour;
    }

    public void SetUIColour(UIEventTypes uIEventTypes)
    {
        if (_images.Length > 0 || _mainText)
        {
            switch (uIEventTypes)
            {
                case UIEventTypes.Normal:
                    ColourChangesProcesses(_normalColour);
                    break;
                case UIEventTypes.Highlighted:
                    if (_highlight)
                    {
                        ColourChangesProcesses(_highlightedColour);
                    }
                    break;
                case UIEventTypes.Selected:
                    if (_selected)
                    {
                        ColourChangesProcesses(_selectedColour);
                    }
                    else
                    {
                        ColourChangesProcesses(_normalColour);
                    }
                    break;
            }
        }
    }

    private void ColourChangesProcesses(Color newColour)
    {
        if (_LerpTextColours || _LerpImageColours)
        {
            MyColourLerper.NewLerp();
        }

        if (_images.Length > 0)
        {
            if (_LerpImageColours)
            {
                MyColourLerper.StartLerp(newColour, _crossFadeTime, (x) => SetImageColour(x));
            }
            else
            {
                foreach (var item in _images)
                {
                    item.color = newColour;
                }
            }
        }

        if (_mainText)
        {
            if (_LerpTextColours)
            {
                MyColourLerper.StartLerp(newColour, _crossFadeTime, (x) => SetTextColour(x));
            }
            else
            {
                _mainText.color = newColour;
            }
        }
    }

    private void SetTextColour(Color newColour)
    {
        _mainText.color = newColour;
    }

    private void SetImageColour(Color newColour)
    {
        foreach (var item in _images)
        {
            item.color = newColour;
        }
    }
}
