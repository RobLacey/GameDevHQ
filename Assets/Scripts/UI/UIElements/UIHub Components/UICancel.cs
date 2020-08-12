using System;

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel
{
    private readonly EscapeKey _globalEscapeSetting;
    private readonly UIDataEvents _uiDataEvents;
    private readonly UIControlsEvents _uiControlsEvents;
    private bool _fromHotKey;
    private UIBranch _currentHomeBranch;

    public UICancel(EscapeKey globalSetting)
    {
        _globalEscapeSetting = globalSetting;
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        OnEnable();
    }

    //Events
    public static event Action<UIBranch> OnBackOneLevel; 

    //Properties
    private UINode LastSelected { get; set; }
    private UIBranch ActiveBranch { get; set; }
    private void SaveSelected(UINode newNode) => LastSelected = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;
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
        if(ActiveBranch.IsResolvePopUp) return;
        
        if (_fromHotKey)
        {
            CancelOrBackButtonPressed(EscapeKey.BackToHome);
        }
        else if (ActiveBranch.IsOptionalPopUp)
        {
            ProcessCancelType(EscapeKey.BackOneLevel);
        }
        else
        {
            ProcessCancelType(LastSelected.HasChildBranch.EscapeKeySetting);
        }
    }

    private void CancelOrBackButtonPressed(EscapeKey escapeKey) 
    {
        if (_fromHotKey)
        {
            _fromHotKey = false;
            LastSelected.SetNotSelected_NoEffects();
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

    private bool NodeDoesntHaveAParent()
    {
        return ActiveBranch == ActiveBranch.MyParentBranch;
    }

    private void StartCancelProcess(Action endOfCancelAction) 
    {
        LastSelected.Audio.Play(UIEventTypes.Cancelled);

        if (IsPopUpOrPauseMenu())
        {
            endOfCancelAction.Invoke();
            return;
        }
        
        StartOutTween(endOfCancelAction);
    }

    private bool IsPopUpOrPauseMenu()
    {
        return ActiveBranch.IsAPopUpBranch() || ActiveBranch.IsPauseMenuBranch();
    }

    private void StartOutTween(Action endAction)
    {
        if (ActiveBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            ActiveBranch.StartOutTween(endAction.Invoke);
        }
        else
        {
            ActiveBranch.StartOutTween();
            endAction.Invoke();
        }
    }

    private void BackOneLevel()
    {
        CancelEvent(LastSelected.MyBranch);
    }

    private void BackToHome()
    {
        LastSelected.SetNotSelected_NoEffects();
        CancelEvent(_currentHomeBranch);
    }

    private void CancelEvent(UIBranch targetBranch)
    {
        OnBackOneLevel?.Invoke(targetBranch);
    }
}
