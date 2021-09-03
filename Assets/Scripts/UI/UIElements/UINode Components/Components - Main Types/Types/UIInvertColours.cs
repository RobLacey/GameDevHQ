using System;
using UnityEngine;
using UnityEngine.UI;

public class UIInvertColours : NodeFunctionBase, IAlwaysHighlightSettings
{
    public UIInvertColours(IInvertSettings settings, IUiEvents uiEvents) : base(uiEvents)
    {
        _settings = settings;
        MyBranch = settings.ParentBranch;
        _activateWhen = settings.ActivateWhen;
        _text = settings.Text;
        _image = settings.Image;
        _invertedColour = settings.Colour;
    }

    //Variables
    private readonly ActivateWhen _activateWhen;
    private readonly Text _text;
    private readonly Image _image;
    private readonly Color _invertedColour;
    private Color _checkMarkStartColour = Color.white;
    private Color _textStartColour = Color.white;
    private bool _hasText;
    private bool _hasImage;
    private IInvertSettings _settings;
    private IAlwaysHighlight _alwaysHighlighted;
    
    //Properties
    protected override bool CanBeHighlighted() => (_activateWhen & ActivateWhen.OnHighlighted) != 0;
    protected override bool CanBePressed() => (_activateWhen & ActivateWhen.OnSelected) != 0;
    public IBranch MyBranch { get; }
    public Override Overridden => _settings.OverrideAlwaysHighlighted;
    public INode UiNode => _uiEvents.ReturnMasterNode;
    public bool IsSelected => _isSelected;
    public Action DoPointerOverSetUp => ChangeToInvertedColour;
    public Action DoPointerNotOver => SetToStartingColour;
    public bool OptionalStartConditions => !CanBeHighlighted();

    //Main
    public override void OnAwake()
    {
        base.OnAwake();
        SetInverseColourSettings();
        _alwaysHighlighted = EZInject.Class.WithParams<IAlwaysHighlight>(this);
    }

    private void SetInverseColourSettings()
    {
        if (_image != null) _checkMarkStartColour = _image.color;
        if (_text != null) _textStartColour = _text.color;
        _hasText = _text;
        _hasImage = _image;
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

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive() || !CanBeHighlighted() || CanBePressed() && _isSelected) return;
        
        if (pointerOver)
        {
            ChangeToInvertedColour();
        }
        else
        {
            if(_alwaysHighlighted.IsNotNull() && _alwaysHighlighted.CanAllow()) return;
            SetToStartingColour();
        }
    }

    private protected override void ProcessPress()
    {
        if (FunctionNotActive() || !CanBePressed()) return;
        if (_isSelected)
        {
            ChangeToInvertedColour();
        }
        else
        { 
            if(CanBeHighlighted()) return;
            SetToStartingColour();
        }
    }

    private protected override void ProcessDisabled()
    {
        if(FunctionNotActive()) return;
        SetToStartingColour();
    }

    private void ChangeToInvertedColour()
    {
        if (_hasImage) _image.color = _invertedColour;
        if (_hasText) _text.color = _invertedColour;
    }

    private void SetToStartingColour()
    {
        if (_hasImage) _image.color = _checkMarkStartColour;
        if (_hasText) _text.color = _textStartColour;
    }
}
