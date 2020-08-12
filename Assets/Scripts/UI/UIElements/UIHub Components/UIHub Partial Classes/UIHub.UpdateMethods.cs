using System.Linq;
using UnityEngine;

/// <summary>
/// This partial Class Looks after the methods needed inside the Update function for UI control
/// </summary>
public partial class UIHub 
{
    private bool CanSwitchBranches() => _noActivePopUps && !MouseOnly();
    private bool CanPauseGame() => _hasPauseAxis && Input.GetButtonDown(_pauseOptionButton);
    private bool CanDoCancel() => _hasCancelAxis && Input.GetButtonDown(_cancelButton);
    private bool CanSwitchBetweenInGameAndMenu() 
        => _hasSwitchToMenuAxis 
           && Input.GetButtonDown(_menuAndGameSwitching.SwitchControls) 
           && _noActivePopUps;
    
    private bool CanEnterPauseWithNothingSelected()
    {
        return (_noActivePopUps && 
                _lastSelected.HasChildBranch.CanvasIsEnabled == false)
               && _pauseOptionsOnEscape == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (_hotKeySettings.Count <= 0) return false;
        if (_gameIsPaused) return false;
        return _noActivePopUps && _hotKeySettings.Any(hotKeys => hotKeys.CheckHotKeys());
    }

    private bool SwitchGroupProcess()
    {
        if (_hasPosSwitchAxis && Input.GetButtonDown(_posSwitchButton))
        {
            OnSwitchGroupsPressed?.Invoke(SwitchType.Positive);
            return true;
        }

        if (_hasNegSwitchAxis && Input.GetButtonDown(_negSwitchButton))
        {
            OnSwitchGroupsPressed?.Invoke(SwitchType.Negative);
            return true;
        }
        return false;
    }
    
    private void PausedPressedActions()
    {
        _gameIsPaused = !_gameIsPaused;
        OnGamePaused?.Invoke(_gameIsPaused);
    }

    private void WhenCancelPressed()
    {
        if (CanEnterPauseWithNothingSelected() || _gameIsPaused)
        {
            PausedPressedActions();
        }
        else
        {
            OnCancelPressed?.Invoke();
        }
    }
}