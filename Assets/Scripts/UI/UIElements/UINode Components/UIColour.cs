using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;

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
    protected override bool FunctionNotActive() => !CanActivate || _scheme is null;
    protected override bool CanBePressed() => (_scheme.ColourSettings & EventType.Pressed) != 0;
    protected override bool CanBeHighlighted() => (_scheme.ColourSettings & EventType.Highlighted) !=0; 
    protected bool CanBeSelected() => (_scheme.ColourSettings & EventType.Selected) != 0;

    public override void OnAwake(UINode node, UiActions uiActions)
    {
        base.OnAwake(node, uiActions);
        CanActivate = (_enabledFunctions & Setting.Colours) != 0;
        _id = node.GetInstanceID();
        _nodesName = node.name;
        SetUpCachedColours();
        CheckForSetUpError();
    }

    private void SetUpCachedColours()
    {
        if (_imageElements.Length > 0)
            _imageNormalColour = _imageElements[0].color;
        
        if (_textElements)
            _textNormalColour = _textElements.color;
        
        _selectHighlightColour = SelectedHighlight();
    }

    private void CheckForSetUpError()
    {
        if (_imageElements.Length == 0 && !_textElements && CanActivate)
            Debug.LogError($"No Image or Text set on Colour settings on {_nodesName}");
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
        if (CanBePressed() && _isSelected)
        {
            if (CanBeHighlighted())
            {
                _tweenImageToColour = SelectedHighlight();
                _tweenTextToColour = SelectedHighlight();
                DoColourChange(_scheme.TweenTime);
            }
            else
            {
                DoSelected();
            }
        }
        else
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

    private protected override void ProcessDisabled(bool isDisabled)
    {
        if (isDisabled)
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
            _tweenImageToColour = SelectedHighlight();
            _tweenTextToColour = SelectedHighlight();
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
