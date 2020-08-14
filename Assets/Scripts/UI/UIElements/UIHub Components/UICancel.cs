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
    private bool _fromHotKey;
    private UIBranch _currentHomeBranch;
    private UINode _lastSelected;
    private UIBranch _activeBranch;

    //Events
    public static event Action<UIBranch> OnBackOneLevel; 

    //Properties
    private void SaveSelected(UINode newNode) => _lastSelected = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    private void SaveFromHotKey() => _fromHotKey = true;
    private void SaveCurrentHomeBranch(UIBranch currentHomeBranch) => _currentHomeBranch = currentHomeBranch;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToCurrentHomeScreen(SaveCurrentHomeBranch);
        _uiControlsEvents.SubscribeFromHotKey(SaveFromHotKey);
        _uiControlsEvents.SubscribeCancelOrBackButtonPressed(CancelOrBackButtonPressed);
        _uiControlsEvents.SubscribeOnCancel(CancelPressed);
    }

    private void CancelPressed()
    {
        if(_activeBranch.IsResolvePopUp) return;
        
        if (_fromHotKey)
        {
            CancelOrBackButtonPressed(EscapeKey.BackToHome);
        }
        else if (_activeBranch.IsOptionalPopUp)
        {
            ProcessCancelType(EscapeKey.BackOneLevel);
        }
        else
        {
            ProcessCancelType(_lastSelected.HasChildBranch.EscapeKeySetting);
        }
    }

    private void CancelOrBackButtonPressed(EscapeKey escapeKey) 
    {
        if (_fromHotKey)
        {
            _fromHotKey = false;
            _lastSelected.SetNotSelected_NoEffects();
        }
        ProcessCancelType(escapeKey);
    }

    private void ProcessCancelType(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.GlobalSetting) escapeKey = _globalEscapeSetting;
        if (NodeDoesntHaveAParent()) return;

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

    private bool NodeDoesntHaveAParent() => _activeBranch == _activeBranch.MyParentBranch;

    private void StartCancelProcess(Action endOfCancelAction) 
    {
        _lastSelected.Audio.Play(UIEventTypes.Cancelled);

        if (IsPopUpOrPauseMenu())
        {
            endOfCancelAction.Invoke();
            return;
        }
        
        StartOutTween(endOfCancelAction);
    }

    private bool IsPopUpOrPauseMenu() => _activeBranch.IsAPopUpBranch() || _activeBranch.IsPauseMenuBranch();

    private void StartOutTween(Action endAction)
    {
        if (_activeBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            _activeBranch.StartOutTween(endAction.Invoke);
        }
        else
        {
            _activeBranch.StartOutTween();
            endAction.Invoke();
        }
    }

    private void BackOneLevel() => InvokeCancelEvent(_lastSelected.MyBranch);

    private void BackToHome()
    {
        _lastSelected.SetNotSelected_NoEffects();
        InvokeCancelEvent(_currentHomeBranch);
    }

    private void InvokeCancelEvent(UIBranch targetBranch) => OnBackOneLevel?.Invoke(targetBranch);
}
