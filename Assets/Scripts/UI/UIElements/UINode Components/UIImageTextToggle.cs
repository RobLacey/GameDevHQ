using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UIImageTextToggle : NodeFunctionBase
{
    [Header("Toggle Ui Image Settings")]
    [SerializeField] private ChangeWhen _changeWhen = ChangeWhen.OnPressed;
    [SerializeField] private Image _toggleIsOff;
    [SerializeField] private Image _toggleIsOn;
    
    [Header("Swapping UI Text Settings")]
    [SerializeField] private Text _textToSwap;
    [SerializeField] private string _changeTextToo;

    // [Header("On Controls Change Settings")] 
    // [SerializeField] private IsActive _onControlChanged = IsActive.No;
    // [SerializeField] private Text _newText;
    // [SerializeField] private Image _defaultImage;
    // [SerializeField] private Image _newImage;

    //variables
    private string _startingText;
    private UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    
    //Properties
    protected override bool CanBeSelected() => _changeWhen == ChangeWhen.OnPressed;
    protected override bool CanBeHighlighted() => _changeWhen == ChangeWhen.OnHighlight;
    protected override bool CanBePressed() => false;
    protected override bool FunctionNotActive() => !CanActivate;
    public bool ToggleOnNewControls => _changeWhen == ChangeWhen.OnControlChanged;

    //Enum
    private enum ChangeWhen { OnHighlight, OnPressed, OnControlChanged }

    public override void OnAwake(UINode node, UiActions uiActions)
    {
        base.OnAwake(node, uiActions);
        CanActivate = ((_enabledFunctions & Setting.SwapImageOrText) != 0) 
                      && CacheAndCheckForStartingUIElements();
        
        CycleToggle(_isSelected);
        _uiControlsEvents.SubscribeToAllowKeys(OnControlsChanged);
    }

    private bool CacheAndCheckForStartingUIElements()
    {
        if (_textToSwap)
            _startingText = _textToSwap.text;
        return _toggleIsOff || _toggleIsOn || _textToSwap;
    }


    private void CycleToggle(bool isOn)
    {
        if (FunctionNotActive()) return;
        
        if (isOn)
        {
            ToggleImages(true);
            ToggleText(true);
        }
        else
        {
            ToggleImages(false);  
            ToggleText(false);
        }
    }

    private void OnControlsChanged(bool keysActive)
    {
        if (FunctionNotActive() || !ToggleOnNewControls) return;
        //TODO Double check and improve this function
        CycleToggle(keysActive);
    }

    private void ToggleText(bool isOn)
    {
        if(_textToSwap is null) return;

        _textToSwap.text = isOn ? _changeTextToo : _startingText;
    }

    private void ToggleImages(bool isOn)
    {
        if(_toggleIsOff is null) return;
        _toggleIsOff.enabled = !isOn;
        _toggleIsOn.enabled = isOn;
    }

    private protected override void ProcessSelectedAndHighLighted() => CycleToggle(true);

    private protected override void ProcessHighlighted() => CycleToggle(true);

    private protected override void ProcessSelected() =>CycleToggle(true);

    private protected override void ProcessToNormal() =>CycleToggle(false);

    private protected override void ProcessPress() => CycleToggle(_isSelected);

    private protected override void ProcessDisabled(bool isDisabled) { }
}
