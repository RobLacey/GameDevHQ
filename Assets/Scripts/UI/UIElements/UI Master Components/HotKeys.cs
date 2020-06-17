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
    public static UIMasterController UIMaster;

    public static void HotKeyActivate(UIBranch branch)
    {
        if (branch.MyBranchType == BranchType.Independent)
        {
            Debug.Log("Can't have independent as HotKey");
            return;
        }

        foreach (var node in branch.MyParentBranch.ThisGroupsUINodes) //****Check this functions corretcly
        {
            if (node.ChildBranch == branch)
            {
                if (UIMaster.LastSelected != node && UIMaster.LastSelected.ChildBranch != null)
                {
                    if (UIMaster.LastSelected.IsSelected == true)
                    {
                        if (UIMaster.LastSelected.ChildBranch.WhenToMove == WhenToMove.OnClick)
                        {
                            UIMaster.LastSelected.ChildBranch.OutTweenToParent();
                            branch.UIMaster.ResetHierachy();
                            TurnOff(branch, node);
                        }
                        else
                        {
                            UIMaster.LastSelected.ChildBranch.OutTweenToParent(() => TurnOff(branch, node));
                            branch.UIMaster.ResetHierachy();
                        }
                    }
                    else
                    {
                        TurnOff(branch, node);
                        branch.UIMaster.ResetHierachy();
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
        if (branch.ScreenType == ScreenType.ToFullScreen)
        {
            if (UIMaster.OnHomeScreen)
            {
                UIHomeGroup.ClearHomeScreen(branch);
            }
        }
        else
        {
            if (!UIMaster.OnHomeScreen)
            {

                UIHomeGroup.RestoreHomeScreen();
            }
        }
        ToNextBranch(branch, node);
    }

    private static void ToNextBranch(UIBranch branch, UINode node)
    {
        if (UIMaster.LastSelected.IsSelected == true)
        {
            UIMaster.LastSelected.Deactivate();
        }
        node.SetSelected_NoEffects();

        if (branch.MyParentBranch.MyBranchType != BranchType.HomeScreenUI)
        {
            branch.FromHotkey = true;
        }
        branch.MyParentBranch.LastHighlighted = node;
        branch.MyParentBranch.LastSelected = node;
        UIMaster.LastSelected = node;
        branch.MyParentBranch.SaveLastSelected(node);
        branch.MoveToNextLevel();
    }
}
