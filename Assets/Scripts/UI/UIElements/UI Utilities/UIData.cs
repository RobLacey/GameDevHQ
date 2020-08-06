using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIData : IMono
{
    public UIData()
    {
        OnEnable();
    }

    public Action<UINode> NewHighLightedNode { private get; set; } 
    public Action<UINode> NewSelectedNode { private get; set; }
    public Action<UIBranch> NewActiveBranch { private get; set; }
    public Action<bool> IsGamePaused{ private get; set; }
    public Action<bool> IsOnHomeScreen { private get; set; }
    public Action<bool> AllowKeys { private get; set; }
    public Action<bool> AmImMenu { private get; set; }
    public Action<bool> NoResolvePopUps { private get; set; }
    public Action<bool> NoNonResolvePopUps { private get; set; }
    public Action RunOnDisable{ private get; set; }
    public Action OnStartUp{ private get; set; }
    
    public void OnEnable()
    {
        PauseMenu.GamePaused += StartPauseMenu;
        UIHub.OnEndOfUse += OnDisable;
        UIHub.OnStart += StartUp;
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        UIBranch.DoActiveBranch += SaveActiveBranch;
        UIHomeGroup.DoOnHomeScreen += SaveOnHomeScreen;
        ChangeControl.DoAllowKeys += SaveAllowKeys;
        MenuAndGameSwitching.IsInTheMenu += SaveInMenu;
        PopUpController.NoResolvePopUps += SaveNoResolvePopUps;
        PopUpController.NoNonResolvePopUps += SaveNoNonResolvePopUps;
    }

    public void OnDisable()
    {
        PauseMenu.GamePaused -= StartPauseMenu;
        UIHub.OnEndOfUse -= OnDisable;
        UIHub.OnStart -= StartUp ;
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        UIBranch.DoActiveBranch -= SaveActiveBranch;
        UIHomeGroup.DoOnHomeScreen -= SaveOnHomeScreen;
        ChangeControl.DoAllowKeys -= SaveAllowKeys;
        MenuAndGameSwitching.IsInTheMenu -= SaveInMenu;

        RunOnDisable?.Invoke();
    }

    private void SaveNoResolvePopUps(bool obj) => NoResolvePopUps?.Invoke(obj);

    private void SaveNoNonResolvePopUps(bool obj) => NoNonResolvePopUps?.Invoke(obj);

    private void SaveInMenu(bool obj) => AmImMenu?.Invoke(obj);

    private void SaveAllowKeys(bool obj) => AllowKeys?.Invoke(obj);

    private void StartUp() => OnStartUp?.Invoke();

    private void SaveOnHomeScreen(bool obj) => IsOnHomeScreen?.Invoke(obj);

    private void SaveActiveBranch(UIBranch obj) => NewActiveBranch?.Invoke(obj);

    private void StartPauseMenu(bool obj) => IsGamePaused?.Invoke(obj);

    private void SaveSelected(UINode obj) => NewSelectedNode?.Invoke(obj);

    private void SaveHighlighted(UINode obj) => NewHighLightedNode?.Invoke(obj);
}
