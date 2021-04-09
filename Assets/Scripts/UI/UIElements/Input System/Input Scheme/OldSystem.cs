using System;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "UIElements Schemes / New Input Scheme - Old", fileName = "Scheme - Old")]
public class OldSystem : InputScheme
{
    [Space(10f, order = 1)]
    [Header("Input Settings", order = 2)] [HorizontalLine(1, color: EColor.Blue, order = 3)]
    
    [SerializeField] 
    [Label("Pause / Option Button")] [InputAxis]
    private string _pauseOptionButton = default;
    [SerializeField]
    [InputAxis] [Label("Next UI Group Object")]
    private string _posSwitchButton = default;
    [SerializeField] 
    [InputAxis] [Label("Previous UI Group Object")]
    private string _negSwitchButton = default;
    [SerializeField] 
    [InputAxis] [Label("Next GOUI Object")]
    private string _posNextGOUIButton = default;
    [SerializeField] 
    [InputAxis] [Label("Previous GOUI Object")]
    private string _negNextGOUIButton = default;
    [SerializeField] 
    [InputAxis] private string _cancelButton = default;
    [SerializeField] 
    [Label("Switch To/From Game Menus")] [InputAxis] 
    private string _switchToMenusButton = default;
    [SerializeField] 
    [InputAxis]
    private string _mouseYAxis = default;
    [SerializeField] 
    [InputAxis] 
    private string _mouseXAxis = default;
    [SerializeField] 
    [Label("Switch To Virtual Cursor")] [InputAxis] 
    private string _switchToVC = default;
    [SerializeField] 
    [Label("Virtual Cursor Horizontal")] [InputAxis] 
    private string _vCursorHorizontal = default;
    [SerializeField] 
    [Label("Virtual Cursor Vertical")] [InputAxis] 
    private string _vCursorVertical = default;
    [SerializeField] [InputAxis] 
    private string _selectButton = default;

    [Space(10f, order = 1)]
    [Header("HotKey Settings", order = 2)] [HorizontalLine(1, color: EColor.Blue, order = 3)]

    [SerializeField] 
    [InputAxis] private string _hotKey1 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey2 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey3 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey4 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey5 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey6 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey7 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey8 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey9 = default;
    [SerializeField] 
    [InputAxis] private string _hotKey0 = default;
    
    //Variables
    private bool _hasPauseAxis,
                 _hasPosSwitchAxis,
                 _hasNegSwitchAxis,
                 _hasPosGOUIAxis,
                 _hasNegGOUIAxis,
                 _hasCancelAxis,
                 _hasSwitchToMenusButton,
                 _hasMouseXAxis,
                 _hasMouseYAxis,
                 _hasSwitchToVCButton,
                 _hasVCursorHorizontal,
                 _hasVCursorVertical,
                 _hasSelectButton;

    private bool _hasHotKey6, _hasHotKey7, _hasHotKey8, _hasHotKey9, _hasHotKey0 
                 , _hasHotKey1, _hasHotKey2, _hasHotKey3, _hasHotKey4, _hasHotKey5;


    //Properties and Setter/Getters
    protected override string PauseButton => _pauseOptionButton;
    protected override string PositiveSwitch => _posSwitchButton;
    protected override string NegativeSwitch => _negSwitchButton;
    protected override string PositiveGOUISwitch => _posNextGOUIButton;
    protected override string NegativeGOUISwitch => _negNextGOUIButton;
    protected override string CancelButton => _cancelButton;
    protected override string MenuToGameSwitch => _switchToMenusButton;
    protected override string VCursorHorizontal => _vCursorHorizontal;
    protected override string SwitchToVC => _switchToVC;
    protected override string MouseXAxis => _mouseXAxis;
    protected override string MouseYAxis => _mouseYAxis;
    protected override string VCursorVertical => _vCursorVertical;
    protected override string SelectedButton => _selectButton;
    public override bool AnyMouseClicked => Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
    public override bool LeftMouseClicked => Input.GetMouseButtonDown(0);
    public override bool RightMouseClicked => Input.GetMouseButtonDown(1);
    public override bool CanSwitchToKeysOrController(bool allowKeys)
    {
        if (ControlType == ControlMethod.MouseOnly) return false;
        
        if (CanUseVirtualCursor && !allowKeys)
        {
            if (VCSwitchTo()) return true;
        }
        return Input.anyKeyDown && !allowKeys;
    }

    public override bool CanSwitchToMouseOrVC(bool allowKeys)
    {
        if (ControlType == ControlMethod.KeysOrControllerOnly) return false;

        if (CanUseVirtualCursor && allowKeys)
        {
            return VCSwitchTo();
        }

        return MouseXAxisValue() != 0 || MouseYAxisValue() != 0;
    }

    public override Vector3 GetMouseOrVcPosition() 
        => CanUseVirtualCursor ? GetVirtualCursorPosition() : Input.mousePosition;
    
    public override void SetVirtualCursorPosition(Vector3 pos) => _virtualCursorPosition = pos;
    private protected override Vector3 GetVirtualCursorPosition() => _virtualCursorPosition;


    //Main
    protected override void SetUpUInputScheme()
    {
        _hasPauseAxis = PauseButton != string.Empty;
        _hasPosSwitchAxis = PositiveSwitch != string.Empty;
        _hasNegSwitchAxis = NegativeSwitch != string.Empty;
        _hasPosGOUIAxis = PositiveSwitch != string.Empty;
        _hasNegGOUIAxis = NegativeSwitch != string.Empty;
        _hasCancelAxis = CancelButton != string.Empty;
        _hasSwitchToMenusButton = MenuToGameSwitch != string.Empty;
        _hasVCursorHorizontal = VCursorHorizontal != string.Empty;
        _hasVCursorVertical = VCursorVertical != string.Empty;
        _hasSwitchToVCButton = SwitchToVC != string.Empty;
        _hasMouseXAxis = MouseXAxis != string.Empty;
        _hasMouseYAxis = MouseYAxis != string.Empty;
        _hasSelectButton = SelectedButton != string.Empty;
        _hasHotKey1 = _hotKey1 != string.Empty;
        _hasHotKey2 = _hotKey2 != string.Empty;
        _hasHotKey3 = _hotKey3 != string.Empty;
        _hasHotKey4 = _hotKey4 != string.Empty;
        _hasHotKey5 = _hotKey5 != string.Empty;
        _hasHotKey6 = _hotKey6 != string.Empty;
        _hasHotKey7 = _hotKey7 != string.Empty;
        _hasHotKey8 = _hotKey8 != string.Empty;
        _hasHotKey9 = _hotKey9 != string.Empty;
        _hasHotKey0 = _hotKey0 != string.Empty;
    }

    public override bool PressPause() => _hasPauseAxis && Input.GetButtonDown(PauseButton);
    public override bool PressedMenuToGameSwitch() 
        => InGameMenuSystem == InGameSystem.On && _hasSwitchToMenusButton && Input.GetButtonDown(MenuToGameSwitch);
    public override bool PressedCancel() => _hasCancelAxis && Input.GetButtonDown(CancelButton);
    public override bool PressedPositiveSwitch() => _hasPosSwitchAxis && Input.GetButtonDown(PositiveSwitch);
    public override bool PressedNegativeSwitch() => _hasNegSwitchAxis && Input.GetButtonDown(NegativeSwitch);
    public override bool PressedPositiveGOUISwitch() => _hasPosGOUIAxis && Input.GetButtonDown(PositiveGOUISwitch);
    public override bool PressedNegativeGOUISwitch()=> _hasNegGOUIAxis && Input.GetButtonDown(NegativeGOUISwitch);
    public override bool VcHorizontalPressed() => _hasVCursorHorizontal && Input.GetButtonDown(VCursorHorizontal);
    public override bool VcVerticalPressed() =>  _hasVCursorVertical && Input.GetButtonDown(VCursorVertical);
    public override float VcHorizontal() => _hasVCursorHorizontal ? Input.GetAxis(VCursorHorizontal) : 0;
    public override float VcVertical() =>  _hasVCursorVertical ? Input.GetAxis(VCursorVertical) : 0;
    public override float MouseXAxisValue() =>  _hasMouseXAxis ? Input.GetAxis(MouseXAxis) : 0;
    public override float MouseYAxisValue() =>  _hasMouseYAxis ? Input.GetAxis(MouseYAxis) : 0;
    private protected override bool VCSwitchTo() => _hasSwitchToVCButton && Input.GetButtonDown(SwitchToVC); 
    public override bool PressSelect() =>  _hasSelectButton && (Input.GetButtonDown(SelectedButton) 
                                                                || Input.GetKeyDown(KeyCode.Return));

    public override bool HotKeyChecker(HotKey hotKey)
    {
        switch (hotKey)    
        {
            case HotKey.HotKey1:
                return _hasHotKey1 & Input.GetButtonDown(_hotKey1);
            case HotKey.HotKey2:
                return _hasHotKey2 & Input.GetButtonDown(_hotKey2);
            case HotKey.HotKey3:
                return _hasHotKey3 & Input.GetButtonDown(_hotKey3);
            case HotKey.HotKey4:
                return _hasHotKey4 & Input.GetButtonDown(_hotKey4);
            case HotKey.HotKey5:
                return _hasHotKey5 & Input.GetButtonDown(_hotKey5);
            case HotKey.HotKey6:
                return _hasHotKey6 & Input.GetButtonDown(_hotKey6);
            case HotKey.HotKey7:
                return _hasHotKey7 & Input.GetButtonDown(_hotKey7);
            case HotKey.HotKey8:
                return _hasHotKey8 & Input.GetButtonDown(_hotKey8);
            case HotKey.HotKey9:
                return _hasHotKey9 & Input.GetButtonDown(_hotKey9);
            case HotKey.HotKey0:
                return _hasHotKey0 & Input.GetButtonDown(_hotKey0);
            default:
                throw new ArgumentOutOfRangeException(nameof(hotKey), hotKey, null);
        }
    }
}