using UnityEngine;
using UnityEngine.UI;

public class UIInvertColours : NodeFunctionBase
{
    public UIInvertColours(IInvertSettings settings, IUiEvents uiEvents)
    {
        _activateWhen = settings.ActivateWhen;
        _text = settings.Text;
        _image = settings.Image;
        _invertedColour = settings.Colour;
        CanActivate = true;
        OnAwake(uiEvents);
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
    
    //Properties
    protected override bool FunctionNotActive() => !_text && !_image && !CanActivate;
    protected override bool CanBeHighlighted() => (_activateWhen & ActivateWhen.OnHighlighted) != 0;
    protected override bool CanBePressed() => (_activateWhen & ActivateWhen.OnSelected) != 0;

    protected sealed override void OnAwake(IUiEvents events)
    {
        base.OnAwake(events);
        SetInverseColourSettings();
    }

    private void SetInverseColourSettings()
    {
        if (_image != null) _checkMarkStartColour = _image.color;
        if (_text != null) _textStartColour = _text.color;
        _hasText = _text;
        _hasImage = _image;
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
            SetToStartingColour();
        }
    }

    private protected override void ProcessPress()
    {
        if (FunctionNotActive() || !CanBePressed()) return;
        if (_isSelected)
        {
        Debug.Log("INVERT");
            ChangeToInvertedColour();
        }
        else
        { 
            if(CanBeHighlighted()) return;
        Debug.Log("rEVERT");
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
