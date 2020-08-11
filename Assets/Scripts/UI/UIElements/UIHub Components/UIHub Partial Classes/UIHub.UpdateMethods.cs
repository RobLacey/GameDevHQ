﻿using System.Linq;
using UnityEngine;

/// <summary>
/// This partial Class Looks after the methods needed inside the Update function for UI control
/// </summary>
public partial class UIHub 
{
    private bool CanSwitchBranches() => _popUpController.NoActivePopUps && !MouseOnly();
    private bool CanPauseGame() => _hasPauseAxis && Input.GetButtonDown(_pauseOptionButton);
    private bool CanDoCancel() => _hasCancelAxis && Input.GetButtonDown(_cancelButton);
    private bool CanSwitchBetweenInGameAndMenu() 
        => _hasSwitchToMenuAxis 
           && Input.GetButtonDown(_menuAndGameSwitching.SwitchControls) 
           && _popUpController.NoActivePopUps;
    
    private bool CanEnterPauseWithNothingSelected()
    {
        return (_popUpController.NoActivePopUps && 
                LastSelected.HasChildBranch.CanvasIsEnabled == false)
               && _pauseOptionsOnEscape == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (_hotKeySettings.Count <= 0) return false;
        if (GameIsPaused) return false;
        return _popUpController.NoActivePopUps && _hotKeySettings.Any(hotKeys => hotKeys.CheckHotKeys());
    }

    private bool SwitchGroupProcess()
    {
        if (_hasPosSwitchAxis && Input.GetButtonDown(_posSwitchButton))
        {
            SwitchingGroups(SwitchType.Positive);
            return true;
        }

        if (_hasNegSwitchAxis && Input.GetButtonDown(_negSwitchButton))
        {
            SwitchingGroups(SwitchType.Negative);
            return true;
        }
        return false;
    }

    private void SwitchingGroups(SwitchType switchType)
    {
        if (_onHomeScreen)
        {
            _uiHomeGroup.SwitchHomeGroups(switchType);
        }
        else
        {
            ActiveBranch.SwitchBranchGroup(switchType);
        }
        LastHighlighted.Audio.Play(UIEventTypes.Selected);
    }
    
    private void PausedPressedActions()
    {
        GameIsPaused = !GameIsPaused;
        OnGamePaused?.Invoke(GameIsPaused);
    }

    private void WhenCancelPressed()
    {
        if (CanEnterPauseWithNothingSelected() || GameIsPaused)
        {
            PausedPressedActions();
        }
        else
        {
            _myUiCancel.CancelPressed();
        }
    }
}