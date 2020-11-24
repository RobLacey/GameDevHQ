using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIColour : NodeFunctionBase
{
    public UIColour(IColourSettings settings, IUiEvents uiEvents)
    {
        _scheme = settings.ColourScheme;
        _textElements = settings.TextElement;
        _imageElements = settings.ImageElement;
        CanActivate = true;
        OnAwake(uiEvents);
    }

    //Variables

    private readonly ColourScheme _scheme;
    private readonly Text _textElements;
    private Image[] _imageElements;
    private Color _textNormalColour = Color.white;
    private Color _imageNormalColour = Color.white;
    private Color _tweenImageToColour;
    private Color _tweenTextToColour;
    private Color _selectHighlightColour;
    private float _selectHighlightPerc;
    private int _id;

    //Properties
    protected override bool FunctionNotActive() => !CanActivate || _scheme is null || _isDisabled;
    protected override bool CanBePressed() => (_scheme.ColourSettings & EventType.Pressed) != 0;
    protected override bool CanBeHighlighted() => (_scheme.ColourSettings & EventType.Highlighted) !=0;
    private bool CanBeSelected() => (_scheme.ColourSettings & EventType.Selected) != 0;

    protected sealed override void OnAwake(IUiEvents uiEvents)
    {
        base.OnAwake(uiEvents);
        _id = uiEvents.MasterNodeID;
        CheckForSetUpError();
        SetUpCachedColours();
    }

    private void SetUpCachedColours()
    {
        if(!CanActivate) return;
        if (_imageElements.Length > 0)
            _imageNormalColour = _imageElements[0].color;
        
        if (_textElements)
            _textNormalColour = _textElements.color;
        
        _selectHighlightColour = SelectedHighlightColour();
    }

    private void CheckForSetUpError() //TODO Improve error handling - Add data to UiActions to gather
    {
        if(!CanActivate) return;
        if (_imageElements.Length > 0 && _imageElements[0] is null)
        {
            _imageElements = new Image[0];
        }
        if (_imageElements.Length == 0 && !_textElements && CanActivate)
            throw new Exception("No Image or Text set on Colour settings");
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if (FunctionNotActive()) return;
        if (pointerOver)
        {
            PointerOverSetUp();
        }
        else
        {
            PointerNotOver();
        }    

    }

    private void PointerOverSetUp()
    {
        if ((CanBePressed() || CanBeSelected()) && _isSelected)
        {
            SelectedHighlight();
        }
        else
        {
            NotSelectedHighlight();
        }
    }

    private void SelectedHighlight()
    {
        if (CanBeHighlighted())
        {
            _tweenImageToColour = SelectedHighlightColour();
            _tweenTextToColour = SelectedHighlightColour();
            DoColourChange(_scheme.TweenTime);
        }
        else
        {
            DoSelected();
        }
    }

    private void NotSelectedHighlight()
    {
        if (CanBeHighlighted())
        {
            DoHighlighted();
        }
        else
        {
            DoNormal();
        }
    }

    private void PointerNotOver()
    {
        if (_isSelected && CanBeSelected())
        {
            DoSelected();
        }
        else
        {
            DoNormal();
        }
    }
    
    private void DoHighlighted()
    {
        _tweenImageToColour = _scheme.HighlightedColour;
        _tweenTextToColour = _scheme.HighlightedColour;
        DoColourChange(_scheme.TweenTime);
    }

    private void DoSelected()
    {
        _tweenImageToColour = _scheme.SelectedColour;
        _tweenTextToColour = _scheme.SelectedColour;
        DoColourChange(_scheme.TweenTime);
    }
    
    private void DoNormal()
    {
        _tweenImageToColour = _imageNormalColour;
        _tweenTextToColour = _textNormalColour;
        DoColourChange(_scheme.TweenTime);
    }

    private protected override void ProcessDisabled()
    {
        if(!CanActivate) return;
        
        if (_isDisabled)
        {
            _tweenImageToColour = _scheme.DisableColour;
            _tweenTextToColour = _scheme.DisableColour;
            DoColourChange(_scheme.TweenTime);
        }
        else
        {
            DoNormal();
        }
    }

    private void DoColourChange(float tweenTime, TweenCallback callback = null)
    {
        ImagesColourChangesProcess(_tweenImageToColour, tweenTime, callback);
        TextColourChangeProcess(_tweenTextToColour, tweenTime, callback);
    }
    
    private protected override void ProcessPress()
    {
        if (FunctionNotActive()) return;

        if (CanBePressed())
        {
            SetUpPressedFlash();
        }
        else
        {
            PointerOverSetUp();
        }
    }

    private void SetUpPressedFlash()
    {
        if (CanBeSelected() && _isSelected)
        {
            _tweenImageToColour = SelectedHighlightColour();
            _tweenTextToColour = SelectedHighlightColour();
        }
        else if (CanBeHighlighted())
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
    
    private Color SelectedHighlightColour()
    {
        bool highlightPercIsTheSame = Mathf.Approximately(_selectHighlightPerc, _scheme.SelectedPerc);
        if (highlightPercIsTheSame) return _selectHighlightColour;
        
        _selectHighlightPerc = _scheme.SelectedPerc;
        float r = ColourCalc(_scheme.SelectedColour.r);
        float g = ColourCalc(_scheme.SelectedColour.g);
        float b = ColourCalc(_scheme.SelectedColour.b);
        float a = ColourCalc(_scheme.SelectedColour.a);
        _selectHighlightColour = new Color(Mathf.Clamp(r * _scheme.SelectedPerc, 0, 1),
                                           Mathf.Clamp(g * _scheme.SelectedPerc, 0, 1),
                                           Mathf.Clamp(b * _scheme.SelectedPerc, 0, 1),
                                           Mathf.Clamp(a * _scheme.SelectedPerc, 0, 1));
        return _selectHighlightColour;
    }

    private float ColourCalc(float value) => value < 0.1f && _scheme.SelectedPerc > 1 ? 0.2f : value;

    private void KillTweens()
    {
        for (int i = 0; i < _imageElements.Length; i++)
        {
            DOTween.Kill("_images" + _id + i);
        }  
    }
}
