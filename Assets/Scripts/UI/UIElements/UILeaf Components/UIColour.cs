using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[System.Serializable]
public class UIColour
{
    [SerializeField] bool _whenSelected;
    [SerializeField] bool _highlight;
    [SerializeField] bool _flashOnPress;
    [SerializeField] bool _TweenImageColours;
    [SerializeField] bool _TweenTextColours;
    [SerializeField] [AllowNesting] [ShowIf(EConditionOperator.Or, "_TweenTextColours", "_TweenImageColours")] 
    float _crossFadeTime = 0.4f;
    [SerializeField] Color _disabled = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] Color _selectedColour = Color.white;
    [SerializeField] [Range(0.5f, 2f)] float _selectedHighlightPerc = 1f;
    [SerializeField] Color _highlightedColour = Color.white;
    [SerializeField] [AllowNesting] [ShowIf("_flashOnPress")] Color _flashColour = Color.white;
    [SerializeField] [AllowNesting] [ShowIf("_flashOnPress")] float _flashTime = 0.1f;
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Header("Assign Elements To Use", order = 0)] Image[] _images;
    [SerializeField] Text _mainText;

    //Variables
    public ColourLerp MyColourLerper { get; set; }
    Color _normalColour = Color.white;

    public void OnAwake()
    {
        if (_images.Length > 0) _normalColour = _images[0].color;
        if (_mainText) _normalColour = _mainText.color;
        if(MyColourLerper != null) MyColourLerper.StartColour = _normalColour;
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

    private void SetAsSelected (bool isSelected)
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
        if (_flashOnPress)
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

    private void OnPressedColourChange(Color tooColour)
    {
        if (_flashOnPress)
        {
            MyColourLerper.NewLerp();

            if (_TweenImageColours || _TweenTextColours)
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

    private void ColourChangesProcesses(Color newColour)
    {
        if (_TweenTextColours || _TweenImageColours) { MyColourLerper.NewLerp(); }

        if (_images.Length > 0)
        {
            if (_TweenImageColours)
            {
                MyColourLerper.StartLerp(newColour, _crossFadeTime, (x) => SetImageColour(x));
            }
            else
            {
                SetImageColour(newColour);
            }
        }

        if (_mainText)
        {
            if (_TweenTextColours)
            {
                MyColourLerper.StartLerp(newColour, _crossFadeTime, (x) => SetTextColour(x));
            }
            else
            {
                SetTextColour(newColour);
            }
        }
    }

    public void SetAsDisabled()
    {
        if (_mainText)
        {
            SetTextColour(_disabled);
        }

        if (_images.Length > 0)
        {
            SetImageColour(_disabled);
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
