using System;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "New Input Scheme - Old", fileName = "Scheme - Old")]
public class OldSystem : InputScheme
{
    [Header("Input Settings")] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [Label("Pause / Option Button")] [InputAxis]
    private string _pauseOptionButton;
    [SerializeField]
    [InputAxis] private string _posSwitchButton;
    [SerializeField] 
    [InputAxis] private string _negSwitchButton;
    [SerializeField] 
    [InputAxis] private string _cancelButton;
    [SerializeField] 
    [Label("Switch To/From Game Menus")] [InputAxis] 
    private string _switchToMenusButton;
    [SerializeField] 
    [InputAxis] private string _hotKey1;
    [SerializeField] 
    [InputAxis] private string _hotKey2;
    [SerializeField] 
    [InputAxis] private string _hotKey3;
    [SerializeField] 
    [InputAxis] private string _hotKey4;

    private bool _hasPauseAxis, _hasPosSwitchAxis, _hasNegSwitchAxis, _hasCancelAxis, _hasSwitchToMenusButton
        , _hasHotKey1, _hasHotKey2, _hasHotKey3, _hasHotKey4;

    //Editor

    private bool InGameOn => _inGameMenuSystem == InGameSystem.On;

    private bool ProtectEscapeKeySetting(EscapeKey escapeKey)
    {
        if (_globalCancelFunction == EscapeKey.GlobalSetting)
        {
            Debug.Log("Escape KeyError");
        }

        return escapeKey != EscapeKey.GlobalSetting;
    }

    public override ControlMethod ControlType => _mainControlType;
    public override PauseOptionsOnEscape PauseOptions => _pauseOptionsOnEscape;
    protected override string PauseButton => _pauseOptionButton;
    protected override string PositiveSwitch => _posSwitchButton;
    protected override string NegativeSwitch => _negSwitchButton;
    protected override string CancelButton => _cancelButton;
    protected override string MenuToGameSwitch => _switchToMenusButton;
    public override InGameSystem InGameMenuSystem => _inGameMenuSystem;
    public override StartInMenu WhereToStartGame => _startGameWhere;
    public override EscapeKey GlobalCancelAction => _globalCancelFunction;
    public override float StartDelay => _atStartDelay;
    public override bool MouseClicked => Input.GetMouseButton(0) || Input.GetMouseButton(1);
    public override bool CanSwitchToKeysOrController => Input.anyKeyDown && ControlType != ControlMethod.MouseOnly;
    public override bool CanSwitchToMouse 
        => _mousePosition != Input.mousePosition && ControlType != ControlMethod.KeysOrControllerOnly;

    public override Vector3 SetMousePosition() => _mousePosition = Input.mousePosition;

    //Main
    public override void OnAwake() => CheckForControls();

    private void CheckForControls()
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
    }


    public override void TurnOffInGameMenuSystem() => _inGameMenuSystem = InGameSystem.Off;

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
            default:
                throw new ArgumentOutOfRangeException(nameof(hotKey), hotKey, null);
        }
    }
}