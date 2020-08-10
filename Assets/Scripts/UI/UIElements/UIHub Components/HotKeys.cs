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
        foreach (UINode parentNode in _myBranch.MyParentBranch.ThisGroupsUiNodes)
        {
            if (parentNode.HasChildBranch != _myBranch) continue;
            StartHotKeyProcess(parentNode);
            SetHotKeyAsSelected(parentNode);
            break;
        }
    }

    private void StartHotKeyProcess(UINode parentNode)
    {
        if (LastSelected.IsSelected)
        {
            StartOutTweenForLastSelected(parentNode);
        }
        else
        {
            StartThisHotKeyBranch(parentNode);
        }
    }

    private void StartOutTweenForLastSelected(UINode parentNode)  
    {
        LastSelected.SetNotSelected_NoEffects();
        if (ActiveBranch.WhenToMove == WhenToMove.Immediately)
        {
            ActiveBranch.StartOutTween();
            StartThisHotKeyBranch(parentNode);
        }
        else
        {
            ActiveBranch.StartOutTween(() => StartThisHotKeyBranch(parentNode));
        }
    }

    private void StartThisHotKeyBranch(UINode parentNode)
    {
        EnsureAlwaysReturnToHomeScreen();
         parentNode.ThisNodeIsSelected();
        _myBranch.MoveToThisBranch();
    }

    private void SetHotKeyAsSelected(UINode parentNode)
    {
        parentNode.SetSelected_NoEffects();
        _myBranch.DefaultStartPosition.Audio.Play(UIEventTypes.Selected);
    }

    private void EnsureAlwaysReturnToHomeScreen()
    {
        if (_myBranch.MyBranchType != BranchType.HomeScreenUI)
             FromHotKey?.Invoke();  //Ensures back to home is used on cancel
    }
}
