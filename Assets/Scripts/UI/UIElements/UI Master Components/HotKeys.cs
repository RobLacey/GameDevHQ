using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class HotKeys 
{
    [InputAxis] [AllowNesting] public string _hotkeyAxis;
    public UIBranch _UIBranch;

    public bool CheckHotkeys()
    {
        if (Input.GetButtonDown(_hotkeyAxis))
        {
            HotKeyProcess.HotKeyActivate(_UIBranch);
            return true;
        }
        return false;
    }
}

public static class HotKeyProcess
{
    public static UIHub _myUIHub;

    public static void HotKeyActivate(UIBranch branch)
    {
        if (branch.MyCanvas.enabled == true) return;

        if (branch.MyBranchType == BranchType.PopUp)
        {
            Debug.Log("Can't have independent as HotKey");
            return;
        }

        foreach (var node in branch.MyParentBranch.ThisGroupsUINodes) //****Check this functions corretcly
        {
            if (node.ChildBranch == branch)
            {
                ToNextBranch(branch, node);
                _myUIHub.LastHighlighted.SetNotHighlighted();

                if (_myUIHub.LastSelected != node && _myUIHub.LastSelected.ChildBranch != null)
                {
                    if (_myUIHub.LastSelected.IsSelected == true)
                    {
                        if (_myUIHub.LastSelected.ChildBranch.WhenToMove == WhenToMove.OnClick)
                        {
                            _myUIHub.LastSelected.ChildBranch.OutTweenToParent();
                            TurnOff(branch, node);
                        }
                        else
                        {
                            _myUIHub.LastSelected.ChildBranch.OutTweenToParent(() => TurnOff(branch, node));
                        }
                    }
                    else
                    {
                        TurnOff(branch, node);
                    }
                }
                else
                {

                    TurnOff(branch, node);
                }
                break;
            }
        }
    } 

    private static void TurnOff(UIBranch branch, UINode node)
    {
        //ToNextBranch(branch, node);

        if (branch.ScreenType == ScreenType.ToFullScreen)
        {
            if (_myUIHub.OnHomeScreen)
            {
                UIHomeGroup.ClearHomeScreen(branch);
                UIHomeGroup.ClearAllPopUpsRegardless();
            }
        }
        else
        {
            if (!_myUIHub.OnHomeScreen)
            {

                UIHomeGroup.RestoreHomeScreen();
            }
        }
        branch.MoveToNextLevel();

    }

    private static void ToNextBranch(UIBranch branch, UINode node)
    {
        UICancel.ResetHierachy();

        if (branch.MyBranchType != BranchType.HomeScreenUI)   { branch.FromHotkey = true; }

        if (branch.ScreenType == ScreenType.ToFullScreen)
        {
            node.IsSelected = true;
        }
        else
        {
            node.SetSelected_NoEffects();
        }
        branch.MyParentBranch.SaveLastHighlighted(node);
        branch.MyParentBranch.SaveLastSelected(node);
        //branch.MoveToNextLevel();
    }
}
