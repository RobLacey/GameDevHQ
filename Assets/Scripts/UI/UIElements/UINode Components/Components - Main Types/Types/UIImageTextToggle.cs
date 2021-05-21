using UnityEngine;
using UnityEngine.UI;

public class UIImageTextToggle : NodeFunctionBase
{
    public UIImageTextToggle(SwapImageOrTextSettings settings, IUiEvents uiEvents) : base(uiEvents)
    {
        _changeWhen = settings.ChangeWhen;
        _toggleIsOff = settings.ToggleOff;
        _toggleIsOn = settings.ToggleOn;
        _textToSwap = settings.TextToSwap;
        _changeTextToo = settings.ChangeTextToo;
        CanActivate = CacheAndCheckForStartingUIElements();
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


    public override void ObserveEvents()
    {
        base.ObserveEvents();
        InputEvents.Do.Subscribe<IAllowKeys>(OnControlsChanged);
    }

    protected override void UnObserveEvents()
    {
        base.UnObserveEvents();
        InputEvents.Do.Unsubscribe<IAllowKeys>(OnControlsChanged);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UnObserveEvents();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        UnObserveEvents();
    }

    protected override void LateStartSetUp()
    {
        base.LateStartSetUp();
        if (MyHubDataIsNull) return;
        
        SetUp();
    }

    public override void OnStart()
    {
        base.OnStart();
        if(!CanActivate) return;
        SetUp();
    }

    private void SetUp()
    {
        if (ToggleOnNewControls)
        {
            CycleToggle(_myDataHub.AllowKeys);
        }
        else
        {
            CycleToggle(_isSelected);
        }
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
        if (!ToggleOnNewControls || !_uiEvents.ReturnMasterNode.MyBranch.CanvasIsEnabled) return;
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
