using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[Serializable]
public class UIInvertColours : NodeFunctionBase
{
    [InfoBox("ONLY set to objects not effected by the Colour effects", EInfoBoxType.Warning)]
    
    [SerializeField]
    private bool _invertOnHighlight;
    [SerializeField] private bool _invertOnSelected;
    [SerializeField] [AllowNesting] [DisableIf("ImageSet")]
    private Text _text;
    [SerializeField] [AllowNesting] [DisableIf("TextSet")]
    private Image _image;
     [SerializeField] private Color _invertedColour = Color.white;

    //Variables
    private Color _checkMarkStartColour = Color.white;
    private Color _textStartColour = Color.white;
    private bool _hasText;
    private bool _hasImage;
    
    //Properties
    protected override bool FunctionNotActive() => !_text && !_image && !CanActivate;
    protected override bool CanBeSelected() => _invertOnSelected;
    protected override bool CanBeHighlighted() => _invertOnHighlight;
    protected override bool CanBePressed() => false;

    // Editor Scripts
    private bool TextSet() => _text != null;
    private bool ImageSet() => _image != null;

    public override void OnAwake(UINode node, UiActions uiActions)
    {
        base.OnAwake(node, uiActions);
        CanActivate = (_enabledFunctions & Setting.InvertColourCorrection) != 0;
        SetInverseColourSettings();
    }

    private void SetInverseColourSettings()
    {
        if (_image != null) _checkMarkStartColour = _image.color;
        if (_text != null) _textStartColour = _text.color;
        _hasText = _text;
        _hasImage = _image;
    }

    private protected override void ProcessSelectedAndHighLighted() => ChangeToInvertedColour(_invertedColour);

    private protected override void ProcessHighlighted() => ChangeToInvertedColour(_invertedColour);

    private protected override void ProcessSelected() => ChangeToInvertedColour(_invertedColour);

    private protected override void ProcessToNormal() => SetToStartingColour();

    private protected override void ProcessPress() { }

    private protected override void ProcessDisabled(bool isDisabled) => SetToStartingColour();

    private void ChangeToInvertedColour(Color newColour)
    {
        if (_hasImage) _image.color = newColour;
        if (_hasText) _text.color = newColour;
    }

    private void SetToStartingColour()
    {
        if (_hasImage) _image.color = _checkMarkStartColour;
        if (_hasText) _text.color = _textStartColour;
    }
}
