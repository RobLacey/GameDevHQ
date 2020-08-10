using System;
using UnityEngine;

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel : IMono
{
    private readonly IPopUpControls _popUpControls;
    private readonly EscapeKey _globalEscapeSetting;
    private readonly UIData _uiData;
    private bool _fromHotKey;

    public UICancel(EscapeKey globalSetting, IPopUpControls popUpControls)
    {
        _popUpControls = popUpControls;
        _globalEscapeSetting = globalSetting;
        _uiData = new UIData();
        OnEnable();
    }

    //Events
    public static event Func<UIBranch> ReturnHomeBranch; 

    //Properties
    private UINode LastSelected { get; set; }
    private UIBranch ActiveBranch { get; set; }
    private void SaveSelected(UINode newNode) => LastSelected = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;
    private void SaveFromHotKey() => _fromHotKey = true;
     
    public void OnEnable()
    {
        _uiData.SubscribeToSelectedNode(SaveSelected);
        _uiData.SubscribeToActiveBranch(SaveActiveBranch);
        _uiData.SubscribeFromHotKey(SaveFromHotKey);
        UINode.DoCancelButtonPressed += CancelOrBackButtonPressed;
    }

    public void OnDisable()
    {
        _uiData.OnDisable();
        UINode.DoCancelButtonPressed -= CancelOrBackButtonPressed;
    }

    public void CancelPressed()
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
        if(!_popUpControls.NoActivePopUps)
        {
            _popUpControls.RemoveNextPopUp();
        }
        else
        {
            LastSelected.SetNotSelected_NoEffects();
            LastSelected.MyBranch.MoveBackToThisBranch();
        }
    }

    private void BackToHome()
    {
        UIBranch homeBranch = ReturnHomeBranch?.Invoke();
        if (homeBranch == null)
        {
            Debug.Log("Not Returning UIBranch");
            return;
        }
        LastSelected.SetNotSelected_NoEffects();
        homeBranch.LastSelected.SetNotSelected_NoEffects();
        homeBranch.MoveBackToThisBranch();
    }
}
