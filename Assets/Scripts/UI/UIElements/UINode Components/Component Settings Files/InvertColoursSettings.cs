using System;
using EZ.Inject;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public interface IComponentSettings: IParameters
{
    NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions);
}

public interface IInvertSettings : IComponentSettings
{
    ActivateWhen ActivateWhen { get; }
    Text Text { get; }
    Image Image { get; }
    Color Colour { get; }
}

[Serializable]
public class InvertColoursSettings : IInvertSettings
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

    // Editor Scripts
    private bool TextSet() => _text != null;
    private bool ImageSet() => _image != null;

    public ActivateWhen ActivateWhen => _activateWhen;
    public Text Text => _text;
    public Image Image => _image;
    public Color Colour => _invertedColour;

    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        if ((functions & Setting.InvertColourCorrection) != 0)
        {
            return new UIInvertColours(this, uiNodeEvents);
        }
        return null;
    }
}
