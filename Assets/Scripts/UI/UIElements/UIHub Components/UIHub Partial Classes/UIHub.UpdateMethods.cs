using System.Linq;
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
                LastSelected.HasChildBranch.MyCanvas.enabled == false)
               && PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (_hotKeySettings.Count <= 0) return false;
        if (GameIsPaused) return false;
        if (!_popUpController.NoActivePopUps) return false;
        return _hotKeySettings.Any(hotKeys => hotKeys.CheckHotKeys());
    }

    private void PauseOptionMenuPressed()
    {
        GameIsPaused = !GameIsPaused;
        if (_pauseMenu)
        {
            _pauseMenu.PauseMenuClass.StartPauseMenu(GameIsPaused);
        }
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
        if (_onHomeScreen && _homeBranches.Count > 1)
        {
            LastHighlighted.Audio.Play(UIEventTypes.Selected);
            _uiHomeGroup.SwitchHomeGroups(switchType);
        }
        else if (ActiveBranch.GroupListCount > 1)
        {
            ActiveBranch.SwitchBranchGroup(switchType);
        }
    }

    private void WhenCancelPressed()
    {
        if (CanEnterPauseWithNothingSelected() || GameIsPaused)
        {
            PauseOptionMenuPressed();
        }
        else
        {
            _myUiCancel.CancelPressed();
        }
    }
}