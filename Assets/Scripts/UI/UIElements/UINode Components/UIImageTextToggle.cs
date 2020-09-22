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

    //variables
    private string _startingText;
    private UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    
    //Properties
    protected override bool CanBeHighlighted() => _changeWhen == ChangeWhen.OnHighlight;
    protected override bool CanBePressed() => _changeWhen == ChangeWhen.OnPressed;
    protected override bool FunctionNotActive() => !CanActivate;
    public bool ToggleOnNewControls => _changeWhen == ChangeWhen.OnControlChanged;

    //Enum
    private enum ChangeWhen { OnHighlight, OnPressed, OnControlChanged }

    public override void OnAwake(UiActions uiActions, Setting activeFunctions)
    {
        base.OnAwake(uiActions, activeFunctions);
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
        if (!ToggleOnNewControls) return;
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

    protected override void SavePointerStatus(bool pointerOver)
    {
        if(FunctionNotActive() || !CanBeHighlighted() || CanBePressed() && _isSelected) return;
        CycleToggle(pointerOver);
    }

    private protected override void ProcessPress()
    {
        if(FunctionNotActive() || !CanBePressed()) return;
        CycleToggle(_isSelected);
    }

    private protected override void ProcessDisabled() { }
}
