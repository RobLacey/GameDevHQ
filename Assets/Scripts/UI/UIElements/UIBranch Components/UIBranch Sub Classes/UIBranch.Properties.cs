using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This partial class holds all the classes properties as UIBranch is an important link between higher and lower parts of the system
/// </summary>
public partial class UIBranch : ICloseBranch
{
    //Set / Getters
    public bool IsAPopUpBranch()
    {
        return _branchType == BranchType.OptionalPopUp
               || _branchType == BranchType.ResolvePopUp;
    }

    public bool IsControlBar() => _controlBar == IsActive.Yes;
    public bool IsPauseMenuBranch() => _branchType == BranchType.PauseMenu;
    public bool IsInternalBranch() => _branchType == BranchType.Internal;
    public bool IsHomeScreenBranch() => _branchType == BranchType.HomeScreen;
    public bool IsStandardBranch() => _branchType == BranchType.Standard;
    public bool IsInGameBranch() => _branchType == BranchType.InGameObject;
    public void DoNotTween()=> _tweenOnChange = false;
    public void DontSetBranchAsActive() => _canActivateBranch = false;
    public bool IsTimedPopUp() => _branchType == BranchType.TimedPopUp;
    public IsActive GetStayOn() => _stayVisible;
    public void SetNotAControlBar() => _controlBar = IsActive.No;
    public IsActive SetStayOn { set => _stayVisible = value; }
    
   //Properties
    public BranchType ReturnBranchType => _branchType;
    public bool CanvasIsEnabled => MyCanvas.enabled;
    public bool CanStoreAndRestoreOptionalPoUp => _storeOrResetOptional == StoreAndRestorePopUps.StoreAndRestore;
    public INode DefaultStartOnThisNode { get; set; }
    public INode LastSelected { get; set; }
    public INode LastHighlighted { get; set; }
    public INode[] ThisGroupsUiNodes { get; set; } = new INode[0];
    public void SetThisGroupsNode(INode[] groupsNodes) => ThisGroupsUiNodes = groupsNodes;
    public IsActive GetSaveOnExit => _saveExitSelection;
    public List<GroupList> BranchGroupsList => _groupsList;
    public int GroupIndex { get; set; }
    public Canvas MyCanvas { get; private set; } 
    public CanvasGroup MyCanvasGroup { get; private set; }
    public IBranch MyParentBranch { get; set; }
    public IBranch ThisBranch => this;
    public GameObject ThisBranchesGameObject => gameObject;
    public IBranch ActiveBranch => this;
    public IAutoOpenClose AutoOpenCloseClass { get; private set; }
    public bool PointerOverBranch => AutoOpenCloseClass.PointerOverBranch;
    public float Timer => _timer;
    public IsActive AlwaysHighlighted => _alwaysHighlighted;

    public IsActive SetSaveLastSelectionOnExit
    {
        set => _saveExitSelection = value;
    }

    public EscapeKey EscapeKeyType
    {
        get => _escapeKeyFunction;
        set => _escapeKeyFunction = value;
    }

    public WhenToMove WhenToMove
    {
        get => _moveType;
        set => _moveType = value;
    }

    public ScreenType ScreenType
    {
        get => _screenType;
        set => _screenType = value;
    }

    public DoTween TweenOnHome
    {
        get => _tweenOnHome;
        set => _tweenOnHome = value;
    }
    
    public IsActive AutoClose
    {
        get =>_autoClose; 
        set => _autoClose = value;
    }

    public float AutoCloseDelay => _autoCloseDelay;

    public OrderInCanvas CanvasOrder
    {
        get => _canvasOrderSetting;
        set => _canvasOrderSetting = value;
    }
    
    public int ReturnManualCanvasOrder => _orderInCanvas;
    public IsActive ReturnOnlyAllowOnHomeScreen => _onlyAllowOnHomeScreen;
}