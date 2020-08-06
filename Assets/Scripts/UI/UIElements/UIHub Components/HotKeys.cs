using System;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

[Serializable]
public class HotKeys
{
    [InputAxis] [AllowNesting] public string _hotKeyAxis;
    [ValidateInput("IsAllowedType", "Can't have PopUp as HotKey as HotKey")] 
    public UIBranch _myBranch;
    
    //Variables
    private UIData _uiData;

    //Properties
    private UINode LastHighlighted { get; set; }
    private UINode LastSelected { get; set; }
    private UIBranch ActiveBranch { get; set; }
    private void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;
    private void SaveSelected(UINode newNode) => LastSelected = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;

    //Editor Script
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
        _uiData.NewHighLightedNode = SaveHighlighted;
        _uiData.NewSelectedNode = SaveSelected;
        _uiData.NewActiveBranch = SaveActiveBranch;
    }

    public bool CheckHotKeys()
    {
        if (!Input.GetButtonDown(_hotKeyAxis)) return false;
        if (_myBranch.MyCanvas.enabled) return false;
        HotKeyActivation();
        return true;
    }

    private void HotKeyActivation()
    {
        foreach (UINode parentNode in _myBranch.MyParentBranch.ThisGroupsUiNodes)
        {
            if (parentNode.HasChildBranch == _myBranch)
            {
                StartHotKeyProcess(parentNode);
                SetHotKeyAsSelected(parentNode);
                break;
            }
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
         parentNode.ThisNodeIsSelected();
        _myBranch.MoveToThisBranch();
    }

    private void SetHotKeyAsSelected(UINode parentNode)
    {
        EnsureAlwaysReturnToHomeScreen();
        parentNode.SetSelected_NoEffects();
        _myBranch.DefaultStartPosition.Audio.Play(UIEventTypes.Selected);
    }

    private void EnsureAlwaysReturnToHomeScreen()
    {
        if (_myBranch.MyBranchType != BranchType.HomeScreenUI)
        {
            _myBranch.FromHotKey = true;
        } //Ensures back to home is used on cancel
    }
}
