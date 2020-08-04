using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This partial Class Looks after the methods needed inside the Update function for UI control
/// </summary>
public partial class UIHub 
{
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
        GamePaused?.Invoke(GameIsPaused);
    }

    public void SwitchBetweenGameAndMenu()
    {
        if (MouseOnly()) return;
        if (!ActiveInGameSystem) return;

        if (InMenu)
        {
            SwitchToGame();
        }
        else
        {
            SwitchToMenu();
        }
        _returnToGameControl.Invoke(InMenu);
        SetInMenu?.Invoke(InMenu);
    }

    private void SwitchToGame()
    {
        InMenu = false;
        LastHighlighted.SetNotHighlighted();
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void SwitchToMenu()
    {
        InMenu = true;
        LastHighlighted.SetAsHighlighted();
        EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
    }

    private bool CanSwitchBranches()
    {
        return _popUpController.NoActivePopUps && !MouseOnly();
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
    
    private bool CanSwitchBetweenInGameAndMenu()
    {
        return _hasSwitchToMenuAxis && Input.GetButtonDown(_switchToMenusButton) && _popUpController.NoActivePopUps;
    }
    
    private bool CanEnterPauseWithNothingSelected()
    {
        return (_popUpController.NoActivePopUps && 
                LastSelected.HasChildBranch.MyCanvas.enabled == false)
               && PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
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
    
    private bool CanPauseGame()
    {
        return _hasPauseAxis && Input.GetButtonDown(_pauseOptionButton);
    }
    
    private bool CanDoCancel()
    {
        return _hasCancelAxis && Input.GetButtonDown(_cancelButton);
    }

}