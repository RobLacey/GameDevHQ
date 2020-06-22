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

        foreach (UINode node in branch.MyParentBranch.ThisGroupsUINodes)
        {
            if (node.ChildBranch == branch)
            {
                ToNextBranch(branch, node);

                if (_myUIHub.LastSelected != node && _myUIHub.LastSelected.ChildBranch != null 
                    && _myUIHub.LastSelected.IsSelected == true)
                {
                    StartTween(branch);
                }
                else
                {

                    TurnOff(branch);
                }
                break;
            }
        }
    }

    private static void StartTween(UIBranch branch)
    {
        if (_myUIHub.LastSelected.ChildBranch.WhenToMove == WhenToMove.OnClick)
        {
            _myUIHub.LastSelected.ChildBranch.OutTweenToParent();
            TurnOff(branch);
        }
        else
        {
            _myUIHub.LastSelected.ChildBranch.OutTweenToParent(() => TurnOff(branch));
        }
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
        branch.DefaultStartPosition.IAudio.Play(UIEventTypes.Selected);
        branch.MyParentBranch.SaveLastHighlighted(node);
        branch.MyParentBranch.SaveLastSelected(node);
    }

    private static void TurnOff(UIBranch branch)
    {
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

}
