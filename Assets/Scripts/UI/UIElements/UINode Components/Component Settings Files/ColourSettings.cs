using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public interface IColourSettings : IComponentSettings, IOverride
{
    ColourScheme ColourScheme { get; }
    Text TextElement { get; }
    Image[] ImageElement { get; }
}

[Serializable]
public class ColourSettings: IColourSettings
{
    [SerializeField] 
    [AllowNesting] [Label("Colour Scheme")] private ColourScheme _scheme;
    [SerializeField] private Override _overrideAlwaysHighlighted = Override.Allow;
    [Header("Elements")]
    [SerializeField] private Text _text;
    [SerializeField] private Image[] _images;

    //Properties, Setters & Getters
    public ColourScheme ColourScheme => _scheme;
    public Text TextElement => _text;
    public Image[] ImageElement => _images;
    public IBranch ParentBranch { get; private set; }
    public Override OverrideAlwaysHighlighted => _overrideAlwaysHighlighted;

    //Main
    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        ParentBranch = uiNodeEvents.ReturnMasterNode.MyBranch;
        CheckForSetUpError(functions, uiNodeEvents.ReturnMasterNode);
        
        if (CanCreate(functions))
        {
            return new UIColour(this, uiNodeEvents);
        }
        return null;
    }

    private bool CanCreate(Setting functions) => (functions & Setting.Colours) != 0;

    private void CheckForSetUpError(Setting functions, UINode parentNode) 
    {
        if(!CanCreate(functions)) return;
        
        if(_scheme.IsNull())
            throw new Exception($"No Scheme set in Colour settings for {parentNode}");

        if (_images.Length > 0 && _images[0] is null)
            throw new Exception($"No Image set in Colour settings for {parentNode}");
        
        if (_images.Length == 0 && !_text)
            throw new Exception($"No Image or Text set in Colour settings for {parentNode}");
    }

}