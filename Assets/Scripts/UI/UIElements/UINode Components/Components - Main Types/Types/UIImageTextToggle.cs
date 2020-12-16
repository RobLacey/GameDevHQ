using UnityEngine;
using UnityEngine.UI;

public class UIImageTextToggle : NodeFunctionBase
{
    public UIImageTextToggle(SwapImageOrTextSettings settings, IUiEvents uiEvents)
    {
        _changeWhen = settings.ChangeWhen;
        _toggleIsOff = settings.ToggleOff;
        _toggleIsOn = settings.ToggleOn;
        _textToSwap = settings.TextToSwap;
        _changeTextToo = settings.ChangeTextToo;
        CanActivate = CacheAndCheckForStartingUIElements();
        OnAwake(uiEvents);
    }

    //variables
    private string _startingText;
    private readonly ChangeWhen _changeWhen;
    private readonly Image _toggleIsOff;
    private readonly Image _toggleIsOn;
    private readonly Text _textToSwap;
    private readonly string _changeTextToo;
    
    //Properties
    protected override bool CanBeHighlighted() => _changeWhen == ChangeWhen.OnHighlight;
    protected override bool CanBePressed() => _changeWhen == ChangeWhen.OnPressed;
    protected override bool FunctionNotActive() => !CanActivate;
    private bool ToggleOnNewControls => _changeWhen == ChangeWhen.OnControlChanged;

    protected sealed override void OnAwake(IUiEvents uiEvents)
    {
        base.OnAwake(uiEvents);
        CycleToggle(_isSelected);
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<IAllowKeys>(OnControlsChanged);
    }

    public override void RemoveEvents()
    {
        base.RemoveEvents();
        Debug.Log("Remove");
        //EVent.Do.Unsubscribe<IAllowKeys>(OnControlsChanged);
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

    private void OnControlsChanged(IAllowKeys args)
    {
        if (!ToggleOnNewControls) return;
        CycleToggle(args.CanAllowKeys);
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
