using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public interface IComponentSettings
{
    NodeFunctionBase SetUp(UiActions uiActions, Setting functions);
}

public interface IColourSettings : IComponentSettings
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
    [SerializeField] private Text _textElements;
    [SerializeField] private Image[] _imageElements;

    public ColourScheme ColourScheme => _scheme;
    public Text TextElement => _textElements;
    public Image[] ImageElement => _imageElements;

    public NodeFunctionBase SetUp(UiActions uiActions, Setting functions)
    {
        if ((functions & Setting.Colours) != 0 && _scheme != null)
        {
            return new UIColour(this, uiActions);
        }
        return new NullFunction();
    }
}