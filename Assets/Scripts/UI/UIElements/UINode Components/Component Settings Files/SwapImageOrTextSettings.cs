using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public interface ISwapImageOrTextSettings : IComponentSettings, IOverride
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
    [SerializeField] private ChangeWhen _changeWhen = ChangeWhen.Never;

    [SerializeField] private Override _overrideAlwaysHighlighted = Override.Allow;
    
    [SerializeField] [AllowNesting] [ShowIf(Active)] private Image _toggleIsOff;
    [SerializeField] [AllowNesting] [ShowIf(Active)]  private Image _toggleIsOn;
    
    [Header("Swapping UI Text Settings")]
    [SerializeField] [AllowNesting] [ShowIf(Active)]  private Text _textToSwap;
    [SerializeField] [AllowNesting] [ShowIf(Active)]  private string _changeTextToo;

    //Editor
    private bool FunctionActive() => _changeWhen != ChangeWhen.Never;
    private const string Active = nameof(FunctionActive);
    
    //Properties, Setters & Getters
    public ChangeWhen ChangeWhen => _changeWhen;
    public Image ToggleOn => _toggleIsOn;
    public Image ToggleOff => _toggleIsOff;
    public Text TextToSwap => _textToSwap;
    public string ChangeTextToo => _changeTextToo;
    public Override OverrideAlwaysHighlighted => _overrideAlwaysHighlighted;
    public IBranch ParentBranch { get; private set; } 


    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        ParentBranch = uiNodeEvents.ReturnMasterNode.MyBranch;

        CheckForSetUpError(functions, uiNodeEvents.ReturnMasterNode);
        
        if (CanCreate(functions))
        {
            return new UISwapImageText(this, uiNodeEvents);
        }
        return null;
    }

    private bool CanCreate(Setting functions) => (functions & Setting.SwapImageOrText) != 0 && FunctionActive();

    private void CheckForSetUpError(Setting functions, UINode parentNode)
    {
        if(!CanCreate(functions)) return;
        
        if(!HasSettings())
            throw new Exception($"Nothing set in Swap Image or Text settings for {parentNode}");
    }

    private bool HasSettings() => _toggleIsOff.IsNotNull() || _toggleIsOn.IsNotNull() || _textToSwap.IsNotNull();
}