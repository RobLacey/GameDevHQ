using System;
using UnityEngine;

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel
{
    public UICancel(EscapeKey globalSetting)
    {
        _globalEscapeSetting = globalSetting;
        OnEnable();
    }

    //Variables
    private readonly EscapeKey _globalEscapeSetting;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private bool _fromHotKey;
    private UIBranch _currentHomeBranch;
    private UINode _lastSelected;
    private UIBranch _activeBranch;
    private bool _noResolvePopUps = true;
    private bool _noPopUps = true;

    //Events
    public static event Action<UIBranch> OnBackOneLevel; 
    public static event Action OnBackOnePopUp; 

    //Properties
    private void SaveSelected(UINode newNode) => _lastSelected = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    private void SaveFromHotKey() => _fromHotKey = true;
    private void SaveCurrentHomeBranch(UIBranch currentHomeBranch) => _currentHomeBranch = currentHomeBranch;
    private void SaveNoResolvePopUps(bool noResolvePopUps) => _noResolvePopUps = noResolvePopUps;
    private void SaveNoPopUps(bool noPopUps) => _noPopUps = noPopUps;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToCurrentHomeScreen(SaveCurrentHomeBranch);
        _uiControlsEvents.SubscribeFromHotKey(SaveFromHotKey);
        _uiControlsEvents.SubscribeCancelOrBackButtonPressed(ProcessCancelType);
        _uiControlsEvents.SubscribeOnCancel(CancelPressed);
        _uiPopUpEvents.SubscribeNoResolvePopUps(SaveNoResolvePopUps);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
    }

    private void CancelPressed()
    {
        if(!_noResolvePopUps) return;
        
        if (_fromHotKey)
        {
            ProcessCancelType(EscapeKey.BackToHome);
        }
        else
        {
            ProcessCancelType(_activeBranch.EscapeKeySetting);
        }
        
    }

    private void ProcessCancelType(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.GlobalSetting) escapeKey = _globalEscapeSetting;

        switch (escapeKey)
        {
            case EscapeKey.BackOneLevel:
                StartCancelProcess(BackOneLevel);
                break;
            case EscapeKey.BackToHome:
                StartCancelProcess(BackToHome);
                break;
        }
    }

    private void StartCancelProcess(Action endOfCancelAction) 
    {
        _activeBranch.LastHighlighted.Audio.Play(UIEventTypes.Cancelled);

        if (!_noPopUps)
        {
            StartOutTween(ToNextPopUp);
        }
        else
        {
            StartOutTween(endOfCancelAction);
        }
    }

    private void StartOutTween(Action endAction)
    {
        _activeBranch.StartOutTweenProcess(endAction);
    }

    private void BackOneLevel() => InvokeCancelEvent(_lastSelected.MyBranch);
    private void ToNextPopUp() => OnBackOnePopUp?.Invoke();

    private void BackToHome()
    {
        InvokeCancelEvent(_currentHomeBranch);
    }

    private void InvokeCancelEvent(UIBranch targetBranch) => OnBackOneLevel?.Invoke(targetBranch);
}
