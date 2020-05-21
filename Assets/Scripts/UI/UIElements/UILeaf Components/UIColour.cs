using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;

[System.Serializable]
public class UIColour
{
    [SerializeField] [EnumFlags] ColourSettings _coloursToUse;
    [SerializeField] [EnumFlags] TweenColours _addTweenTo;
    [SerializeField] [Range(0,2)] float _tweenTime = 0.4f;
    [SerializeField] [Range(0.5f, 2f)] float _selectedHighlightPerc = 1f;
    [SerializeField] [Range(0, 0.5f)] float _flashTime = 0.1f;
    [Header("Colours", order = 0)]
    [SerializeField] Color _disabled = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] Color _selected = Color.white;
    [SerializeField] Color _highlighted = Color.white;
    [SerializeField] Color _pressedFlash = Color.white;
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] [Header("Assign Elements To Use", order = 0)] Text _textElements;
    [SerializeField] Image[] _imageElements;

    //Variables
    Color _normalColour = Color.white;
    Setting _mySetting = Setting.Colours;
    int _id;
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
        if (!((setting & _mySetting) != 0)) return;

        if ((_coloursToUse & ColourSettings.Highlighted) !=0)
        {
            if ((_coloursToUse & ColourSettings.Selected) != 0 && isSelected)
            {
                ColourChangesProcesses(SelectedHighlight(), _tweenTime);
            }
            else
            {
                ColourChangesProcesses(_highlighted, _tweenTime);
            }
        }    
    }

    public void SetColourOnExit(bool isSelected, Setting setting)
    {
        if (!((setting & _mySetting) != 0)) return;

        if ((_coloursToUse & ColourSettings.Selected) == 0 || !isSelected)
        {
            ColourChangesProcesses(_normalColour, _tweenTime);
        }

        if ((_coloursToUse & ColourSettings.Selected) != 0 && isSelected)
        {
            ColourChangesProcesses(_selected, _tweenTime);
        }
    }

    public void ResetToNormal(Setting setting)
    {
        if (!((setting & _mySetting) != 0)) return;
        ColourChangesProcesses(_normalColour, _tweenTime);
    }

    public void ProcessPress(bool isSelected, Setting setting)
    {
        if (!((setting & _mySetting) != 0)) return;

        if ((_coloursToUse & ColourSettings.Pressed) != 0)
        {
            if (isSelected)
            {
                if ((_coloursToUse & ColourSettings.Selected) != 0)
                {
                    ColourChangesProcesses(_pressedFlash, _flashTime, ()=> ColourChangesProcesses(_selected, _flashTime));
                }
                else if((_coloursToUse & ColourSettings.Highlighted) != 0)
                {
                    ColourChangesProcesses(_pressedFlash, _flashTime, () => ColourChangesProcesses(_highlighted, _flashTime));
                }
                else
                {
                    ColourChangesProcesses(_pressedFlash, _flashTime, () => ColourChangesProcesses(_normalColour, _flashTime));
                }
            }
            else
            {
                if ((_coloursToUse & ColourSettings.Highlighted) != 0)
                {
                    ColourChangesProcesses(_pressedFlash, _flashTime, () => ColourChangesProcesses(_highlighted, _flashTime));
                }
                else
                {
                    ColourChangesProcesses(_pressedFlash, _flashTime, () => ColourChangesProcesses(_normalColour, _flashTime));
                }
            }
        }
        else
        {
            SetAsSelected(isSelected);
        }
    }

    private void SetAsSelected(bool isSelected)
    {
        if ((_coloursToUse & ColourSettings.Selected) != 0)
        {
            if (isSelected)
            {
                ColourChangesProcesses(_selected, _tweenTime);
            }

            if (!isSelected)
            {
                if ((_coloursToUse & ColourSettings.Highlighted) != 0)
                {
                    ColourChangesProcesses(_highlighted, _tweenTime);
                }
                else
                {
                    ColourChangesProcesses(_normalColour, _tweenTime);
                }
            }
        }
    }

    private void ColourChangesProcesses(Color newColour, float time, TweenCallback tweenCallback = null)
    {
        KillTweens();

        if (_imageElements.Length > 0)
        {
            if ((_addTweenTo & TweenColours.Images) != 0)
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
            else
            {
                SetImagesColour(newColour);
            }
        }

        if (_textElements)
        {
            if ((_addTweenTo & TweenColours.Text) != 0)
            {
                _textElements.DOColor(newColour, time)
                         .SetId("_mainText" + _id)
                         .SetEase(Ease.Linear)
                         .SetAutoKill(true)
                         .OnComplete(tweenCallback)
                         .Play();
            }
            else
            {
                _textElements.color = newColour;
            }
        }
    }

    public void SetAsDisabled(Setting setting)
    {
        if (!((setting & _mySetting) != 0)) return;

        if (_textElements)
        {
            _textElements.color =_disabled;
        }

        if (_imageElements.Length > 0)
        {
            SetImagesColour(_disabled);
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
        float r = ColourCalc(_selected.r);
        float g = ColourCalc(_selected.g);
        float b = ColourCalc(_selected.b);
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

    private void KillTweens()
    {
        DOTween.Kill("_mainText" + _id);

        for (int i = 0; i < _imageElements.Length; i++)
        {
            DOTween.Kill("_images" + _id + i);
        }  
    }
}
