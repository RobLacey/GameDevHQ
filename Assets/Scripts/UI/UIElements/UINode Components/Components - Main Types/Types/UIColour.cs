using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIColour : NodeFunctionBase, IAlwaysHighlightSettings
{
    public UIColour(IColourSettings settings, IUiEvents uiEvents) : base(uiEvents)
    {
        _settings = settings;
        _scheme = _settings.ColourScheme;
        _textElements = _settings.TextElement;
        _imageElements = _settings.ImageElement;
        MyBranch = _settings.ParentBranch;
    }

    //Variables

    private readonly ColourScheme _scheme;
    private readonly Text _textElements;
    private readonly Image[] _imageElements;
    private Color _textNormalColour = Color.white;
    private Color _imageNormalColour = Color.white;
    private Color _tweenImageToColour;
    private Color _tweenTextToColour;
    private Color _selectHighlightColour;
    private float _selectHighlightPerc;
    private int _id;
    private readonly IColourSettings _settings;
    private IAlwaysHighlight _alwaysHighlighted;

    //Properties, Getter & Setters
    public IBranch MyBranch { get; }
    protected override bool CanBePressed() => (_scheme.ColourSettings & EventType.Pressed) != 0;
    protected override bool CanBeHighlighted() => (_scheme.ColourSettings & EventType.Highlighted) !=0;
    private bool CanBeSelected() => (_scheme.ColourSettings & EventType.Selected) != 0;
    public Override Overridden => _settings.OverrideAlwaysHighlighted;
    public INode UiNode => _uiEvents.ReturnMasterNode;
    public bool IsSelected => _isSelected;
    public Action DoPointerOverSetUp => PointerOverSetUp;
    public Action DoPointerNotOver => PointerNotOver;
    public bool OptionalStartConditions => !CanBeHighlighted();

    public override void OnAwake()
    {
        base.OnAwake();
        _id = _uiEvents.MasterNodeID;
        SetUpCachedColours();
        if(MyBranch.AlwaysHighlighted == IsActive.Yes)
            _alwaysHighlighted = EZInject.Class.WithParams<IAlwaysHighlight>(this);
    }
    
    private void SetUpCachedColours()
    {
        if (_imageElements.Length > 0)
            _imageNormalColour = _imageElements[0].color;
        
        if (_textElements)
            _textNormalColour = _textElements.color;
        
        _selectHighlightColour = SelectedHighlightColour();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if(_alwaysHighlighted.IsNotNull())
            _alwaysHighlighted.ObserveEvents();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if(_alwaysHighlighted.IsNotNull())
            _alwaysHighlighted.UnObserveEvents();
    }

    protected override void LateStartSetUp()
    {
        base.LateStartSetUp();
        if(MyHubDataIsNull) return;
        
        if (_myDataHub.SceneStarted)
        {
            _alwaysHighlighted.ShowStartingHighlightedNode();
        }
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if (pointerOver)
        {
            PointerOverSetUp();
        }
        else
        {
            if (_alwaysHighlighted.IsNotNull() && _alwaysHighlighted.CanAllow()) return;
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