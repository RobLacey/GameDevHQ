using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Serialization;

[System.Serializable]
public class HotKeys 
{
    [InputAxis] [AllowNesting] public string _hotKeyAxis;
    [ValidateInput("IsAllowedType", "Can't have PopUp as HotKey as HotKey")] public UIBranch _UIBranch;

    //Variables
    IHubData _myUIHub;
   // ICancel _myUICancel;
    IHomeGroup _homeGroup;

    //Editor Script
    #region Editor Script
    public bool IsAllowedType()
    {
        if (_UIBranch.IsAPopUpBranch())
        {
            Debug.Log("Can't have PopUp as Hotkey as HotKey");
            return false;
        }
        return true;
    }
    #endregion

    public void OnAwake(IHubData hubData,IHomeGroup homeGroup)
    {
        _myUIHub = hubData;
        _homeGroup = homeGroup;
    }

    public bool CheckHotKeys()
    {
        if (!Input.GetButtonDown(_hotKeyAxis)) return false;
        HotKeyActivate();
        return true;
    }

    private void HotKeyActivate()
    {
        if (_UIBranch.MyCanvas.enabled) return;

        foreach (UINode node in _UIBranch.MyParentBranch.ThisGroupsUINodes)
        {
            if (node.ChildBranch == _UIBranch)
            {
                if (TweenToHotKey(node))
                {
                    StartOutTweenOnLastSelected(node);
                }
                else
                {
                    StartHotKeyBranch(node);
                }
                SetUpNextBranch(node);
                break;
            }
        }
    }

    private bool TweenToHotKey(UINode node)
    {
        return _myUIHub.LastSelected != node && _myUIHub.LastSelected.ChildBranch != null
                            && _myUIHub.LastSelected.IsSelected;
    }

    private void StartOutTweenOnLastSelected(UINode parentNode)
    {
        if (_myUIHub.LastSelected.ChildBranch.WhenToMove == WhenToMove.OnClick)
        {
            _myUIHub.LastSelected.ChildBranch.StartOutTween();
            StartHotKeyBranch(parentNode);
        }
        else
        {
            _myUIHub.LastSelected.ChildBranch.StartOutTween(() => StartHotKeyBranch(parentNode));
        }
    }

    private void SetUpNextBranch(UINode parentNode)
    {
        if (_UIBranch.MyBranchType != BranchType.HomeScreenUI) { _UIBranch.FromHotkey = true; }                //Ensures back to home is used on cancel

        if (_UIBranch.ScreenType == ScreenType.ToFullScreen)
        {
            parentNode.IsSelected = true;
        }
        else
        {
            parentNode.SetSelected_NoEffects();
        }
        _UIBranch.DefaultStartPosition.IAudio.Play(UIEventTypes.Selected);
        _UIBranch.MyParentBranch.SaveLastHighlighted(parentNode);
    }

    private void StartHotKeyBranch(UINode parentNode)
    {
        _homeGroup.SetHomeGroupIndex(parentNode.MyBranch);
        _UIBranch.MyParentBranch.SaveLastSelected(parentNode);
        _UIBranch.MoveToNextLevel();
    }
}
