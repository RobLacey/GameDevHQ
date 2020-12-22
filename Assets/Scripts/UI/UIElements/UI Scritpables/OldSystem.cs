﻿using System;
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
    [InputAxis] private string _posSwitchButton = default;
    [SerializeField] 
    [InputAxis] private string _negSwitchButton = default;
    [SerializeField] 
    [InputAxis] private string _cancelButton = default;
    [SerializeField] 
    [Label("Switch To/From Game Menus")] [InputAxis] 
    private string _switchToMenusButton = default;
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
    private bool _hasPauseAxis, _hasPosSwitchAxis, _hasNegSwitchAxis, _hasCancelAxis, _hasSwitchToMenusButton
        , _hasHotKey1, _hasHotKey2, _hasHotKey3, _hasHotKey4, _hasHotKey5;

    private bool _hasHotKey6, _hasHotKey7, _hasHotKey8, _hasHotKey9, _hasHotKey0;

    //Properties and Setter/Getters
    protected override string PauseButton => _pauseOptionButton;
    protected override string PositiveSwitch => _posSwitchButton;
    protected override string NegativeSwitch => _negSwitchButton;
    protected override string CancelButton => _cancelButton;
    protected override string MenuToGameSwitch => _switchToMenusButton;
    public override bool MouseClicked => Input.GetMouseButton(0) || Input.GetMouseButton(1);
    public override bool CanSwitchToKeysOrController => Input.anyKeyDown && ControlType != ControlMethod.MouseOnly;
    public override bool CanSwitchToMouse 
        => _mousePosition != Input.mousePosition && ControlType != ControlMethod.KeysOrControllerOnly;

    public override void SetMousePosition() => _mousePosition = Input.mousePosition;

    //Main
    protected override void SetUpUInputScheme()
    {
        _hasPauseAxis = PauseButton != string.Empty;
        _hasPosSwitchAxis = PositiveSwitch != string.Empty;
        _hasNegSwitchAxis = NegativeSwitch != string.Empty;
        _hasCancelAxis = CancelButton != string.Empty;
        _hasSwitchToMenusButton = MenuToGameSwitch != string.Empty;
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