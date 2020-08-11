using System;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class HotKeys : IMono
{
    [InputAxis] [AllowNesting] public string _hotKeyAxis;
    [ValidateInput("IsAllowedType", "Can't have PopUp as HotKey as HotKey")] 
    public UIBranch _myBranch;
    
    //Variables
    private UIData _uiData;
    private UINode _parentNode;
    private bool _hasParentNode;
    private bool _notHomeScreenHotKey;

    //Properties
    private UINode LastSelected { get; set; }
    private UIBranch ActiveBranch { get; set; }
    private void SaveSelected(UINode newNode) => LastSelected = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;

    //Events
    public static event Action FromHotKey;
    
    //EditorScript
    public bool IsAllowedType()
    {
        if (!_myBranch.IsAPopUpBranch()) return true;
        Debug.Log("Can't have PopUp as Hot Key as Hot Key");
        return false;
    }

    public void OnAwake()
    {
        _uiData = new UIData();
        _notHomeScreenHotKey = _myBranch.MyBranchType != BranchType.HomeScreenUI;
        OnEnable();
    }
    
    public void OnEnable()
    {
        _uiData.SubscribeToSelectedNode(SaveSelected);
        _uiData.SubscribeToActiveBranch(SaveActiveBranch);
    }

    public void OnDisable()
    {
        _uiData.OnDisable();
    }

    public bool CheckHotKeys()
    {
        if (!Input.GetButtonDown(_hotKeyAxis)) return false;
        if (_myBranch.CanvasIsEnabled) return false;
        HotKeyActivation();
        return true;
    }

    private void HotKeyActivation()
    {    
        if(!_hasParentNode)
        {
            GetParentNode();
        }
        StartHotKeyProcess();
        SetHotKeyAsSelectedActions();

    }

    private void GetParentNode()
    {
        foreach (UINode parentNode in _myBranch.MyParentBranch.ThisGroupsUiNodes)
        {
            if (parentNode.HasChildBranch != _myBranch) continue;
            _hasParentNode = true;
            _parentNode = parentNode;
            break;
        }
    }

    private void StartHotKeyProcess()
    {
        if (LastSelected.IsSelected)
        {
            StartOutTweenForLastSelected();
        }
        else
        {
            StartThisHotKeyBranch();
        }
    }

    private void StartOutTweenForLastSelected()  
    {
        LastSelected.SetNotSelected_NoEffects();
        if (ActiveBranch.WhenToMove == WhenToMove.Immediately)
        {
            ActiveBranch.StartOutTween();
            StartThisHotKeyBranch();
        }
        else
        {
            ActiveBranch.StartOutTween(() => StartThisHotKeyBranch());
        }
    }

    private void StartThisHotKeyBranch()
    {
        EnsureAlwaysReturnToHomeScreen();
        _myBranch.MoveToThisBranch();
    }

    private void SetHotKeyAsSelectedActions()
    {
        _parentNode.ThisNodeIsSelected();
        _parentNode.SetSelected_NoEffects();
        _myBranch.DefaultStartPosition.Audio.Play(UIEventTypes.Selected);
    }

    private void EnsureAlwaysReturnToHomeScreen()
    {
        if (_notHomeScreenHotKey)
             FromHotKey?.Invoke();  //Ensures back to home is used on cancel
    }
}
