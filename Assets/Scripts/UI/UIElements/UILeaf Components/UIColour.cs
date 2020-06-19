using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;

[System.Serializable]
public class UIColour 
{
    [SerializeField] [AllowNesting] [Label("Colour Scheme")] ColourScheme _scheme;
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Header("UI Elements To Use (MUST Assign at Least One)", order = 0)] Text _textElements;
    [SerializeField] Image[] _imageElements;

    //Variables
    Color _normalColour = Color.white;
    Setting _mySetting = Setting.Colours;
    int _id;

    public bool UsingColourScheme()
    {
        return _scheme;
    }

    public bool NoSettings { get; set; } 

    public void OnAwake(int objectId)
    {
        _id = objectId;
        if (_imageElements.Length > 0)
        { 
            if(_imageElements[0] == null)
            {
                _imageElements = new Image[0];
            }
            else
            {
                _normalColour = _imageElements[0].color; 
            }
        }
        if (_textElements) _normalColour = _textElements.color;
        if (_imageElements.Length == 0 && !_textElements) { NoSettings = true; }
    }

    public void SetColourOnEnter(bool isSelected, Setting setting)
    {
        if (EffectNotSet(setting) || _scheme == null) return;

        if ((_scheme.ColourSettings & EventType.Highlighted) !=0)
        {
            if ((_scheme.ColourSettings & EventType.Selected) != 0 && isSelected)
            {
                ColourChangesProcesses(SelectedHighlight(), _scheme.TweenTime);
            }
            else
            {
                ColourChangesProcesses(_scheme.HighlightedColour, _scheme.TweenTime);
            }
        }    
    }

    public void SetColourOnExit(bool isSelected, Setting setting)
    {
        if (EffectNotSet(setting) || _scheme == null) return;

        if ((_scheme.ColourSettings & EventType.Selected) == 0 || !isSelected)
        {
            ColourChangesProcesses(_normalColour, _scheme.TweenTime);
        }

        if ((_scheme.ColourSettings & EventType.Selected) != 0 && isSelected)
        {
            ColourChangesProcesses(_scheme.SelectedColour, _scheme.TweenTime);
        }
    }

    public void ResetToNormal(Setting setting)
    {
        if (EffectNotSet(setting) || _scheme == null) return;
        ColourChangesProcesses(_normalColour, _scheme.TweenTime);
    }

    public void ProcessPress(bool isSelected, Setting setting)
    {
        if (EffectNotSet(setting) || _scheme == null) return;

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
        Color flashtoo;

        if ((_scheme.ColourSettings & EventType.Highlighted) != 0)
        {
            flashtoo = _scheme.HighlightedColour;
        }
        else
        {
            flashtoo = _normalColour;
        }

        ColourChangesProcesses(_scheme.PressedColour, _scheme.FlashTime,
                                () => ColourChangesProcesses(flashtoo, _scheme.FlashTime));
    }

    private void PressedEffect_IsSelected()
    {
        Color flashtoo;

        if ((_scheme.ColourSettings & EventType.Selected) != 0)
        {
            flashtoo = _scheme.SelectedColour;
        }
        else if ((_scheme.ColourSettings & EventType.Highlighted) != 0)
        {
            flashtoo = _scheme.HighlightedColour;
        }
        else
        {
            flashtoo = _normalColour;
        }

        ColourChangesProcesses(_scheme.PressedColour, _scheme.FlashTime,
                               () => ColourChangesProcesses(flashtoo, _scheme.FlashTime));
    }

    private void SetAsSelected(bool isSelected)
{
    if ((_scheme.ColourSettings & EventType.Selected) != 0)
    {
        if (isSelected)
        {
            ColourChangesProcesses(_scheme.SelectedColour, _scheme.TweenTime);
        }

        if (!isSelected)
        {
            if ((_scheme.ColourSettings & EventType.Highlighted) != 0)
            {
                ColourChangesProcesses(_scheme.HighlightedColour, _scheme.TweenTime);
            }
            else
            {
                ColourChangesProcesses(_normalColour, _scheme.TweenTime);
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

    public void SetAsDisabled(Setting setting)
    {
        if (EffectNotSet(setting) || _scheme == null) return;

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
        DOTween.Kill("_mainText" + _id);

        for (int i = 0; i < _imageElements.Length; i++)
        {
            DOTween.Kill("_images" + _id + i);
        }  
    }

    private bool EffectNotSet(Setting setting)
    {
        return !((setting & _mySetting) != 0);
    }

}
