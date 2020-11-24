using System;
using UnityEngine;
using UnityEngine.UI;

public interface ISwapImageOrTextSettings : IComponentSettings
{
    ChangeWhen ChangeWhen { get; }
    Image ToggleOn { get; }
    Image ToggleOff { get; }
    Text TextToSwap { get; }
    String ChangeTextToo { get; }
}

[Serializable]
public class SwapImageOrTextSettings : ISwapImageOrTextSettings
{
    [Header("Toggle Ui Image Settings")]
    [SerializeField] private ChangeWhen _changeWhen = ChangeWhen.OnPressed;
    [SerializeField] private Image _toggleIsOff;
    [SerializeField] private Image _toggleIsOn;
    
    [Header("Swapping UI Text Settings")]
    [SerializeField] private Text _textToSwap;
    [SerializeField] private string _changeTextToo;

    public ChangeWhen ChangeWhen => _changeWhen;
    public Image ToggleOn => _toggleIsOn;
    public Image ToggleOff => _toggleIsOff;
    public Text TextToSwap => _textToSwap;
    public string ChangeTextToo => _changeTextToo;

    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        if ((functions & Setting.SwapImageOrText) != 0)
        {
            return new UIImageTextToggle(this, uiNodeEvents);
        }
        return new NullFunction();
    }
}