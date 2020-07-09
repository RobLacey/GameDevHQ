using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class HotKeys 
{
    [InputAxis] [AllowNesting] public string _hotKeyAxis;
    [ValidateInput("IsAllowedType", "Can't have PopUp as HotKey as HotKey")] public UIBranch _uiBranch;

    //Variables
    private UIHub _uIHub;
    private UIHomeGroup _homeGroup;

    //Editor Script
    #region Editor Script
    public bool IsAllowedType()
    {
        if (!_uiBranch.IsAPopUpBranch()) return true;
        Debug.Log("Can't have PopUp as Hot Key as Hot Key");
        return false;
    }
    #endregion

    public void OnAwake(UIHub hubData, UIHomeGroup homeGroup)
    {
        _uIHub = hubData;
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
        if (_uiBranch.MyCanvas.enabled) return;

        foreach (UINode node in _uiBranch.MyParentBranch.ThisGroupsUiNodes)
        {
            if (node.ChildBranch != _uiBranch) continue;
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

    private bool TweenToHotKey(UINode node)
    {
        return _uIHub.LastSelected != node && _uIHub.LastSelected.ChildBranch != null
                            && _uIHub.LastSelected.IsSelected;
    }

    private void StartOutTweenOnLastSelected(UINode parentNode)
    {
        if (_uIHub.LastSelected.ChildBranch.WhenToMove == WhenToMove.Immediately)
        {
            _uIHub.LastSelected.ChildBranch.StartOutTween();
            StartHotKeyBranch(parentNode);
        }
        else
        {
            _uIHub.LastSelected.ChildBranch.StartOutTween(() => StartHotKeyBranch(parentNode));
        }
    }

    private void SetUpNextBranch(UINode parentNode)
    {
        if (_uiBranch.MyBranchType != BranchType.HomeScreenUI) { _uiBranch.FromHotKey = true; }                //Ensures back to home is used on cancel

        if (_uiBranch.ScreenType == ScreenType.FullScreen)
        {
            parentNode.IsSelected = true;
        }
        else
        {
            parentNode.SetSelected_NoEffects();
        }
        _uiBranch.DefaultStartPosition.Audio.Play(UIEventTypes.Selected);
        _uiBranch.MyParentBranch.SaveLastHighlighted(parentNode);
    }

    private void StartHotKeyBranch(UINode parentNode)
    {
        _homeGroup.SetHomeGroupIndex(parentNode.MyBranch);
        _uiBranch.MyParentBranch.SaveLastSelected(parentNode);
        _uiBranch.MoveToThisBranch();
    }
}
