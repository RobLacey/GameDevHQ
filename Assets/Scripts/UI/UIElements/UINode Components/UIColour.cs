using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;
using TMPro.EditorUtilities;

// ReSharper disable IdentifierTypo

[Serializable]
public class UIColour : NodeFunctionBase
{
    [SerializeField] [AllowNesting] [Label("Colour Scheme")]
    private ColourScheme _scheme;
    [HorizontalLine(4, EColor.Blue, order = 1)]
    [SerializeField] [Header("UI Elements To Use (MUST Assign at Least One)", order = 0)]
    private Text _textElements;
    [SerializeField] private Image[] _imageElements;

    //Variables
    private Color _textNormalColour = Color.white;
    private Color _imageNormalColour = Color.white;
    private Color _tweenImageToColour;
    private Color _tweenTextToColour;
    private Color _selectHighlightColour;
    private float _selectHighlightPerc;
    private int _id;
    private string _nodesName;

    //Properties
    private bool ColourFunctionIsActive => !CanActivate || _scheme is null;
    private bool CanBeSelected => (_scheme.ColourSettings & EventType.Selected) != 0;
    private bool CanBePressed => (_scheme.ColourSettings & EventType.Pressed) != 0;
    private bool CanBeHighlighted => (_scheme.ColourSettings & EventType.Highlighted) !=0; 
    private protected override void SavePointerStatus(bool pointerOver) => _pointerOver = pointerOver;
    private protected override void SaveIsSelected(bool isSelected)
    {
        if (ColourFunctionIsActive) return;
        _isSelected = isSelected;
        SaveHighlighted(false);
    }
    
    private protected override void SaveIsPressed(bool pressed)
    {
        if (ColourFunctionIsActive) return;
        if (!CanBePressed) return;
        _isPressed = pressed;
        ProcessPress();
    }

    public override void OnAwake(UINode node, Setting setting, Actions actions)
    {
        base.OnAwake(node, setting, actions);
        CanActivate = (setting & Setting.Colours) != 0;
        _id = node.GetInstanceID();
        _nodesName = node.name;
        SetNormalColours();
        CheckForSetUpError();
        _selectHighlightColour = SelectedHighlight();
    }

    private void SetNormalColours()
    {
        if (_imageElements.Length > 0)
            _imageNormalColour = _imageElements[0].color;

        if (_textElements)
            _textNormalColour = _textElements.color;
    }

    private void CheckForSetUpError()
    {
        if (_imageElements.Length == 0 && !_textElements && CanActivate)
        {
            Debug.LogError($"No Image or Text set on Colour settings on {_nodesName}");
        }
    }

    private protected override void SaveHighlighted(bool isHighlighted)
    {
        if (ColourFunctionIsActive) return;
        
        _isHighlighted = _pointerOver || isHighlighted;
        
        if(_isPressed) return;
        
        if (_isHighlighted)
        {
            SetHighlighted();
        }
        else
        {
            SetNotHighlighted();
        }
    }


    private void SetHighlighted()
    {
        if (CanBeSelected && _isSelected)
        {
            ProcessSelectedAndHighLighted();
        }
        else if(CanBeHighlighted)
        {
            ProcessHighlighted();
        }
    }

    private void SetNotHighlighted()
    {
        if (CanBeSelected && _isSelected)
        {
            ProcessSelected();
        }
        else
        {
           ProcessToNormal();
        }
    }

    private void ProcessSelectedAndHighLighted()
    {
        _tweenImageToColour = SelectedHighlight();
        _tweenTextToColour = SelectedHighlight();
        DoColourChange(_scheme.TweenTime);
    }
    
    private void ProcessHighlighted()
    {
        _tweenImageToColour = _scheme.HighlightedColour;
        _tweenTextToColour = _scheme.HighlightedColour;
        DoColourChange(_scheme.TweenTime);
    }

    private void ProcessSelected()
    {
        _tweenImageToColour = _scheme.SelectedColour;
        _tweenTextToColour = _scheme.SelectedColour;
        DoColourChange(_scheme.TweenTime);
    }
    
    private void ProcessToNormal()
    {
        _tweenImageToColour = _imageNormalColour;
        _tweenTextToColour = _textNormalColour;
        DoColourChange(_scheme.TweenTime);
    }
    
    private void DoColourChange(float tweenTime, TweenCallback callback = null)
    {
        ImagesColourChangesProcess(_tweenImageToColour, tweenTime, callback);
        TextColourChangeProcess(_tweenTextToColour, tweenTime, callback);
    }

    public void ResetToNormalColour()
    {
        if (ColourFunctionIsActive) return;
        
        ProcessToNormal();
    }

    private void ProcessPress()
    {
        if (CanBeSelected && _isSelected)
        {
            _tweenImageToColour = SelectedHighlight();
            _tweenTextToColour = SelectedHighlight();
        }
        else if (CanBeHighlighted)
        {
            _tweenImageToColour = _scheme.HighlightedColour;
            _tweenTextToColour = _scheme.HighlightedColour;
        }
        else
        {
            _tweenImageToColour = _imageNormalColour;
            _tweenTextToColour = _textNormalColour;
        }
        DoFlashEffect();
    }

    private void DoFlashEffect()
    {
        ImagesColourChangesProcess(_scheme.PressedColour, _scheme.FlashTime, FinishFlashCallBack);
        TextColourChangeProcess(_scheme.PressedColour, _scheme.FlashTime, FinishFlashCallBack);
        
        void FinishFlashCallBack() => DoColourChange(_scheme.FlashTime);
    }


    private void ImagesColourChangesProcess(Color newColour, float time, TweenCallback tweenCallback = null)
    {
        KillTweens();
        if (_imageElements.Length <= 0) return;
        
        for (int i = 0; i < _imageElements.Length; i++)
        {
            _imageElements[i].DOColor(newColour, time)
                             .SetId($"_images{_id}{i}")
                             .SetEase(Ease.Linear)
                             .SetAutoKill(true)
                             .OnComplete(tweenCallback)
                             .Play();
        }
    }

    private void TextColourChangeProcess(Color newColour, float time, TweenCallback tweenCallback = null)
    {
        DOTween.Kill($"_mainText{_id}");
        if (!_textElements) return;
        
        _textElements.DOColor(newColour, time)
                     .SetId($"_mainText{_id}")
                     .SetEase(Ease.Linear)
                     .SetAutoKill(true)
                     .OnComplete(tweenCallback)
                     .Play();
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
        bool highlightPercIsTheSame = Mathf.Approximately(_selectHighlightPerc, _scheme.SelectedPerc);
        if (highlightPercIsTheSame) return _selectHighlightColour;
        
        _selectHighlightPerc = _scheme.SelectedPerc;
        float r = ColourCalc(_scheme.SelectedColour.r);
        float g = ColourCalc(_scheme.SelectedColour.g);
        float b = ColourCalc(_scheme.SelectedColour.b);
        _selectHighlightColour = new Color(Mathf.Clamp(r * _scheme.SelectedPerc, 0, 1),
                                           Mathf.Clamp(g * _scheme.SelectedPerc, 0, 1),
                                           Mathf.Clamp(b * _scheme.SelectedPerc, 0, 1));
        return _selectHighlightColour;

    }

    private float ColourCalc(float value)
    {
        if (value < 0.1f && _scheme.SelectedPerc > 1)
        {
            return 0.2f;
        }
        return value;
    }

    private void KillTweens()
    {
        for (int i = 0; i < _imageElements.Length; i++)
        {
            DOTween.Kill("_images" + _id + i);
        }  
    }
}
