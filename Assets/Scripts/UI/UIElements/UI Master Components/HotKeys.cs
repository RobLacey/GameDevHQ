using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class HotKeys 
{
    [InputAxis] [AllowNesting] public string _hotKeyAxis;
    [ValidateInput("IsAllowedType", "Can't have PopUp as HotKey as HotKey")] public UIBranch _uiBranch;

    //Variables
    private IHubData _myUiHub;
    private IHomeGroup _homeGroup;

    //Editor Script
    #region Editor Script
    public bool IsAllowedType()
    {
        if (!_uiBranch.IsAPopUpBranch()) return true;
        Debug.Log("Can't have PopUp as Hot Key as Hot Key");
        return false;
    }
    #endregion

    public void OnAwake(IHubData hubData,IHomeGroup homeGroup)
    {
        _myUiHub = hubData;
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

        foreach (UINode node in _uiBranch.MyParentBranch.ThisGroupsUINodes)
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
        return _myUiHub.LastSelected != node && _myUiHub.LastSelected.ChildBranch != null
                            && _myUiHub.LastSelected.IsSelected;
    }

    private void StartOutTweenOnLastSelected(UINode parentNode)
    {
        if (_myUiHub.LastSelected.ChildBranch.WhenToMove == WhenToMove.OnClick)
        {
            _myUiHub.LastSelected.ChildBranch.StartOutTween();
            StartHotKeyBranch(parentNode);
        }
        else
        {
            _myUiHub.LastSelected.ChildBranch.StartOutTween(() => StartHotKeyBranch(parentNode));
        }
    }

    private void SetUpNextBranch(UINode parentNode)
    {
        if (_uiBranch.MyBranchType != BranchType.HomeScreenUI) { _uiBranch.FromHotkey = true; }                //Ensures back to home is used on cancel

        if (_uiBranch.ScreenType == ScreenType.ToFullScreen)
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
        _uiBranch.MoveToNextLevel();
    }
}
