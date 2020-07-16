using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;
// ReSharper disable IdentifierTypo

[Serializable]
public class UIColour 
{
    [SerializeField] [AllowNesting] [Label("Colour Scheme")] ColourScheme _scheme;
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Header("UI Elements To Use (MUST Assign at Least One)", order = 0)] Text _textElements;
    [SerializeField] Image[] _imageElements;

    //Variables
    Color _textNormalColour = Color.white;
    Color _imageNormalColour = Color.white;
    int _id;

    //Editor Methods
    public bool UsingColourScheme()  { return _scheme; }

    //Properties
    public bool NoSettings { get; set; } 
    public bool CanActivate { get; private set; }

    public void OnAwake(int objectId, Setting setting)
    {
        CanActivate = (setting & Setting.Colours) != 0;
        _id = objectId;
        SetNormalColours();
        if (_imageElements.Length == 0 && !_textElements) { NoSettings = true; }
    }

    private void SetNormalColours()
    {
        if (_imageElements.Length > 0)
        {
            _imageNormalColour = _imageElements[0].color;
        }

        if (_textElements)
        {
            _textNormalColour = _textElements.color;
        }
    }

    public void SetColourOnEnter(bool isSelected)
    {
        if (!CanActivate || _scheme == null) return;

        if ((_scheme.ColourSettings & EventType.Highlighted) !=0)
        {
            if ((_scheme.ColourSettings & EventType.Selected) != 0 && isSelected)
            {
                ColourChangesProcesses(SelectedHighlight(), _scheme.TweenTime);
                TextColourChangeProcess(SelectedHighlight(), _scheme.TweenTime);
            }
            else
            {
                ColourChangesProcesses(_scheme.HighlightedColour, _scheme.TweenTime);
                TextColourChangeProcess(_scheme.HighlightedColour, _scheme.TweenTime);
            }
        }    
    }

    public void SetColourOnExit(bool isSelected)
    {
        if (!CanActivate || _scheme == null) return;

        if ((_scheme.ColourSettings & EventType.Selected) == 0 || !isSelected)
        {
            ColourChangesProcesses(_imageNormalColour, _scheme.TweenTime);
            TextColourChangeProcess(_textNormalColour, _scheme.TweenTime);
        }

        if ((_scheme.ColourSettings & EventType.Selected) != 0 && isSelected)
        {
            ColourChangesProcesses(_scheme.SelectedColour, _scheme.TweenTime);
            TextColourChangeProcess(_scheme.SelectedColour, _scheme.TweenTime);
        }
    }

    public void ResetToNormalColour()
    {
        if (!CanActivate || _scheme == null) return;
        ColourChangesProcesses(_imageNormalColour, _scheme.TweenTime);
        TextColourChangeProcess(_textNormalColour, _scheme.TweenTime);
    }

    public void ProcessPress(bool isSelected)
    {
        if (!CanActivate || _scheme == null) return;

        if ((_scheme.ColourSettings & EventType.Pressed) != 0)
        {
            if (isSelected)
            {
                PressedEffect_IsSelected();
            }
            else
            {
                PressedEffect_NotSelected();
            }
        }
        else
        {
            SetAsSelected(isSelected);
        }
    }

    private void PressedEffect_NotSelected()
    {
        Color flashtooImage;
        Color flashtooText;

        if ((_scheme.ColourSettings & EventType.Highlighted) != 0)
        {
            flashtooImage = _scheme.HighlightedColour;
            flashtooText = _scheme.HighlightedColour;
        }
        else
        {
            flashtooImage = _imageNormalColour;
            flashtooText = _textNormalColour;
        }

        ColourChangesProcesses(_scheme.PressedColour, _scheme.FlashTime,
                                () => ColourChangesProcesses(flashtooImage, _scheme.FlashTime));
        TextColourChangeProcess(_scheme.PressedColour, _scheme.FlashTime,
                                () => ColourChangesProcesses(flashtooText, _scheme.FlashTime));
    }

    private void PressedEffect_IsSelected()
    {
        Color flashtooImage;
        Color flashtooText;

        if ((_scheme.ColourSettings & EventType.Selected) != 0)
        {
            flashtooImage = _scheme.SelectedColour;
            flashtooText = _scheme.SelectedColour;
        }
        else if ((_scheme.ColourSettings & EventType.Highlighted) != 0)
        {
            flashtooImage = _scheme.HighlightedColour;
            flashtooText = _scheme.HighlightedColour;
        }
        else
        {
            flashtooImage = _imageNormalColour;
            flashtooText = _textNormalColour;
        }

        ColourChangesProcesses(_scheme.PressedColour, _scheme.FlashTime,
                               () => ColourChangesProcesses(flashtooImage, _scheme.FlashTime));
        TextColourChangeProcess(_scheme.PressedColour, _scheme.FlashTime,
                               () => ColourChangesProcesses(flashtooText, _scheme.FlashTime));
    }

    private void SetAsSelected(bool isSelected)
{
    if ((_scheme.ColourSettings & EventType.Selected) != 0)
    {
        if (isSelected)
        {
            ColourChangesProcesses(_scheme.SelectedColour, _scheme.TweenTime);
            TextColourChangeProcess(_scheme.SelectedColour, _scheme.TweenTime);
        }

        if (!isSelected)
        {
            if ((_scheme.ColourSettings & EventType.Highlighted) != 0)
            {
                ColourChangesProcesses(_scheme.HighlightedColour, _scheme.TweenTime);
                TextColourChangeProcess(_scheme.HighlightedColour, _scheme.TweenTime);
            }
            else
            {
                ColourChangesProcesses(_imageNormalColour, _scheme.TweenTime);
                TextColourChangeProcess(_textNormalColour, _scheme.TweenTime);
            }
        }
    }
}

    private void ColourChangesProcesses(Color newColour, float time, TweenCallback tweenCallback = null)
    {
        KillTweens();

        if (_imageElements.Length > 0)
        {
            for (int i = 0; i < _imageElements.Length; i++)
            {
                _imageElements[i].DOColor(newColour, time)
                                                .SetId("_images" + _id + i)
                                                .SetEase(Ease.Linear)
                                                .SetAutoKill(true)
                                                .OnComplete(tweenCallback)
                                                .Play();
            }               
        }
    }

    private void TextColourChangeProcess(Color newColour, float time, TweenCallback tweenCallback = null)
    {
        DOTween.Kill("_mainText" + _id);

        if (_textElements)
        {
            _textElements.DOColor(newColour, time)
                         .SetId("_mainText" + _id)
                         .SetEase(Ease.Linear)
                         .SetAutoKill(true)
                         .OnComplete(tweenCallback)
                         .Play();
        }
    }

    public void SetAsDisabled()
    {
        if (!CanActivate || _scheme == null) return;

        if (_textElements)
        {
            _textElements.color = _scheme.DisableColour;
        }

        if (_imageElements.Length > 0)
        {
            SetImagesColour(_scheme.DisableColour);
        }
    }

    private void SetImagesColour(Color newColour)
    {
        foreach (var item in _imageElements)
        {
            item.color = newColour;
        }
    }

    private Color SelectedHighlight()
    {
        float r = ColourCalc(_scheme.SelectedColour.r);
        float g = ColourCalc(_scheme.SelectedColour.g);
        float b = ColourCalc(_scheme.SelectedColour.b);
        return new Color(Mathf.Clamp(r * _scheme.SelectedPerc, 0, 1),
                         Mathf.Clamp(g * _scheme.SelectedPerc, 0, 1),
                         Mathf.Clamp(b * _scheme.SelectedPerc, 0, 1));
    }

    private float ColourCalc(float value)
    {
        if (value < 0.1f && _scheme.SelectedPerc > 1)
        {
            return 0.2f;
        }
        else
        {
            return value;
        }
    }

    private void KillTweens()
    {
        for (int i = 0; i < _imageElements.Length; i++)
        {
            DOTween.Kill("_images" + _id + i);
        }  
    }
}
