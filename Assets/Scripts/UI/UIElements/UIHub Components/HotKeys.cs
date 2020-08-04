using System;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class HotKeys : IMono
{
    [InputAxis] [AllowNesting] public string _hotKeyAxis;
    [ValidateInput("IsAllowedType", "Can't have PopUp as HotKey as HotKey")] public UIBranch _uiBranch;

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
        if (!_uiBranch.IsAPopUpBranch()) return true;
        Debug.Log("Can't have PopUp as Hot Key as Hot Key");
        return false;
    }

    public void OnAwake()
    {
        OnEnable();
    }
    
    public void OnEnable()
    {
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        UIBranch.DoActiveBranch += SaveActiveBranch;
    }

    public void OnDisable()
    {
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        UIBranch.DoActiveBranch -= SaveActiveBranch;
    }

    public bool CheckHotKeys()
    {
        if (!Input.GetButtonDown(_hotKeyAxis)) return false;
        if (_uiBranch.MyCanvas.enabled) return false;
        HotKeyActivation();
        return true;
    }

    private void HotKeyActivation()
    {
        foreach (UINode parentNode in _uiBranch.MyParentBranch.ThisGroupsUiNodes)
        {
            if (parentNode.HasChildBranch == _uiBranch)
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
        _uiBranch.MoveToThisBranch();
    }

    private void SetHotKeyAsSelected(UINode parentNode)
    {
        EnsureAlwaysReturnToHomeScreen();
        parentNode.SetSelected_NoEffects();
        _uiBranch.DefaultStartPosition.Audio.Play(UIEventTypes.Selected);
    }

    private void EnsureAlwaysReturnToHomeScreen()
    {
        if (_uiBranch.MyBranchType != BranchType.HomeScreenUI)
        {
            _uiBranch.FromHotKey = true;
        } //Ensures back to home is used on cancel
    }
}
