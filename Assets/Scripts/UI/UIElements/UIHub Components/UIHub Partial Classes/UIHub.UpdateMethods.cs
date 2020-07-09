using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// This partial Class Looks after the methods needed inside the Update function for UI control
/// </summary>
public partial class UIHub 
{
    private bool HotKeyPressed()
    {
        if (!CheckIfHotKeyAllowed()) return false;
        _activatedHotKey = _hotKeySettings.Any(hotKeys => hotKeys.CheckHotKeys());
        if (!_activatedHotKey) return _activatedHotKey;
        if (!InMenu) GameToMenuSwitching();
        return _activatedHotKey;
    }

    private bool CheckIfHotKeyAllowed()
    {
        if (_hotKeySettings.Count <= 0) return false;
        if (ActivePopUpsResolve.Count > 0 || GameIsPaused) return false;
        if (_changeControl.UsingKeysOrCtrl && !NoActivePopUps) return false;
        return true;
    }

    public void PauseOptionMenuPressed()
    {
        _pauseOptionMenu.PauseMenuClass.PauseMenu();
        IsPaused?.Invoke(GameIsPaused);
    }

    public void GameToMenuSwitching()
    {
        if (MouseOnly()) return;
        if (!ActiveInGameSystem) return;

        if (InMenu)
        {
            InMenu = false;
            LastHighlighted.SetNotHighlighted();
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            InMenu = true;
            LastHighlighted.SetAsHighlighted();
            EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
        }
        _returnToGameControl.Invoke(InMenu);
    }

    private bool CanSwitchBranches()
    {
        return ActivePopUpsResolve.Count == 0 && !MouseOnly();
    }

    private void SwitchingGroups(SwitchType switchType)
    {
        if (ActivePopUpsNonResolve.Count > 0)
        {
            ActiveNextPopUp();
        }
        else if (OnHomeScreen && _homeBranches.Count > 1)
        {
            LastHighlighted.Audio.Play(UIEventTypes.Selected);
            _uiHomeGroup.SwitchHomeGroups(switchType);
        }
        else if (ActiveBranch.GroupListCount > 1)
        {
            ActiveBranch.SwitchBranchGroup(switchType);
        }
    }
}