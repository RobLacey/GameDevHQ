using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UIImageTextToggle : NodeFunctionBase
{
    [Header("Toggle Ui Image Settings")]
    [SerializeField] private ChangeWhen _changeWhen = ChangeWhen.OnPressed;
    [SerializeField] Image _toggleIsOff;
    [SerializeField] Image _toggleIsOn;
    [Header("Swapping UI Text Settings")]
    [SerializeField]  Text _textToSwap;
    [SerializeField] string _changeTextToo;

    //variables
    private string _startingText;
    
    //Properties
    protected override bool CanBeSelected() => _changeWhen == ChangeWhen.OnPressed;
    protected override bool CanBeHighlighted() => _changeWhen == ChangeWhen.OnHighlight;
    protected override bool CanBePressed() => false;
    protected override bool FunctionNotActive() => !CanActivate;

    //Enum
    private enum ChangeWhen { OnHighlight, OnPressed }

    public override void OnAwake(UINode node, UiActions uiActions)
    {
        base.OnAwake(node, uiActions);
        CanActivate = ((_enabledFunctions & Setting.SwapImageOrText) != 0) 
                      && CacheAndCheckForStartingUIElements();
        
        CycleToggle(_isSelected);
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
