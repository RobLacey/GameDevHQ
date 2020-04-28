using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIColour
{
    public Image[] _images;
    public Text _mainText;
    public bool _LerpImageColours;
    public bool _LerpTextColours;
    public float _crossFadeTime = 0.4f;
    public Color _selectedColour = Color.white;
    [SerializeField] [Range(0.5f, 2f)] float _selectedHighlightPerc = 1f;
    public Color _highlightedColour = Color.white;
    public bool _highlight;
    public bool _whenSelected;
    public bool _pressFlash;
    public Color _flashColour = Color.white;
    public float _flashTime = 0.1f;

    //Variables
    public ColourLerp MyColourLerper { get; set; }
    Color _normalColour = Color.white;

    public void OnAwake()
    {
        if (_images.Length > 0) _normalColour = _images[0].color;
        if (_mainText) _normalColour = _mainText.color;
        if(MyColourLerper != null) MyColourLerper.StartColour = _normalColour;
    }

    private void OnPressedColourChange(Color tooColour)
    {
        if (_pressFlash)
        {
            MyColourLerper.NewLerp();

            if (_LerpImageColours || _LerpTextColours)
            {

                if (_images.Length > 0)
                {
                    MyColourLerper.StartFlash_Lerp(_flashColour, _flashTime, (x) => SetImageColour(x), tooColour);
                }

                if (_mainText)
                {
                    MyColourLerper.StartFlash_Lerp(_flashColour, _flashTime, (x) => SetTextColour(x), tooColour);
                }
            }
            else
            {
                if (_images.Length > 0)
                {
                    MyColourLerper.StartFlash_NonLerp(_flashColour, _flashTime, (x) => SetImageColour(x), tooColour);
                }

                if (_mainText)
                {
                    MyColourLerper.StartFlash_NonLerp(_flashColour, _flashTime, (x) => SetTextColour(x), tooColour);
                }

            }
        }
    }

    public void SetColourOnEnter(bool isSelected)
    {
        if (_highlight)
        {
            if (_whenSelected && isSelected)
            {
                ColourChangesProcesses(SelectedHighlight());
                return;
            }
            ColourChangesProcesses(_highlightedColour);
        }    
    }

    public void SetColourOnExit(bool isSelected)
    {
        if (!_whenSelected || !isSelected)
        {
            ColourChangesProcesses(_normalColour);
        }

        if (_whenSelected && isSelected)
        {
            ColourChangesProcesses(_selectedColour);
        }
    }

    public void ResetToNormal()
    {
        ColourChangesProcesses(_normalColour);
    }

    public void SetAsSelected (bool isSelected)
    {
        if (_whenSelected)
        {
            if (isSelected)
            {
                ColourChangesProcesses(_selectedColour);
            }

            if (!isSelected)
            {
                if (_highlight)
                {
                    ColourChangesProcesses(_highlightedColour);
                }
                else
                {
                    ColourChangesProcesses(_normalColour);
                }
            }
        }
    }

    public void ProcessPress(bool isSelected)
    {
        if (_pressFlash)
        {
            if (isSelected)
            {
                if (_whenSelected)
                {
                    OnPressedColourChange(_selectedColour);
                }
                else if(_highlight)
                {
                    OnPressedColourChange(_highlightedColour);
                }
                else
                {
                    OnPressedColourChange(_normalColour);
                }
            }
            else
            {
                if (_highlight)
                {
                    OnPressedColourChange(_highlightedColour);
                }
                else
                {
                    OnPressedColourChange(_normalColour);
                }
            }
        }
        else
        {
            SetAsSelected(isSelected);
        }
    }

    private void ColourChangesProcesses(Color newColour)
    {
        if (_LerpTextColours || _LerpImageColours) { MyColourLerper.NewLerp(); }

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

    private Color SelectedHighlight()
    {
        float r = ColourCalc(_selectedColour.r);
        float g = ColourCalc(_selectedColour.g);
        float b = ColourCalc(_selectedColour.b);
        return new Color(Mathf.Clamp(r * _selectedHighlightPerc, 0, 1),
                  Mathf.Clamp(g * _selectedHighlightPerc, 0, 1),
                  Mathf.Clamp(b * _selectedHighlightPerc, 0, 1));
    }

    private float ColourCalc(float value)
    {
        if (value < 0.1f && _selectedHighlightPerc > 1)
        {
            return 0.2f;
        }
        else
        {
            return value;
        }
    }
}
