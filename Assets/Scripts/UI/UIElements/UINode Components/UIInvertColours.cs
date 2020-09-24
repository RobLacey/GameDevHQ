﻿using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[Serializable]
public class UIInvertColours : NodeFunctionBase
{
    [InfoBox("ONLY set to objects not effected by the Colour effects", EInfoBoxType.Warning)]
    
    [SerializeField]
    private ActivateWhen _activateWhen;
    [SerializeField] 
    [AllowNesting] [DisableIf("ImageSet")] private Text _text;
    [SerializeField] 
    [AllowNesting] [DisableIf("TextSet")] private Image _image;
    [SerializeField] 
    private Color _invertedColour = Color.white;

    //Variables
    private Color _checkMarkStartColour = Color.white;
    private Color _textStartColour = Color.white;
    private bool _hasText;
    private bool _hasImage;

    [Flags]
    private enum ActivateWhen
    {
        None = 0,
        OnHighlighted = 1 << 0,
        OnSelected = 2 <<1
    }
    
    //Properties
    protected override bool FunctionNotActive() => !_text && !_image && !CanActivate;
    protected override bool CanBeHighlighted() => (_activateWhen & ActivateWhen.OnHighlighted) != 0;
    protected override bool CanBePressed() => (_activateWhen & ActivateWhen.OnSelected) != 0;

    // Editor Scripts
    private bool TextSet() => _text != null;
    private bool ImageSet() => _image != null;

    public override void OnAwake(UiActions uiActions, Setting activeFunctions)
    {
        base.OnAwake(uiActions, activeFunctions);
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
