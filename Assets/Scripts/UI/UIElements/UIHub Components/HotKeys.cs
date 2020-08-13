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
    private UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private UIPopUpEvents _uiPopUpEvents;
    private UINode _parentNode;
    private bool _hasParentNode;
    private bool _notHomeScreenHotKey;
    private bool _gameIsPaused;
    private bool _noActivePopUps = true;

    //Properties
    private UINode LastSelected { get; set; }
    private UIBranch ActiveBranch { get; set; }
    private void SaveSelected(UINode newNode) => LastSelected = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;
    private void SaveIsPaused(bool isPaused) => _gameIsPaused = isPaused;
    private void SaveNoActivePopUps(bool noaActivePopUps) => _noActivePopUps = noaActivePopUps;

    //Events
    public static event Action FromHotKey;
    
    //EditorScript
    private bool IsAllowedType()
    {
        if (!_myBranch.IsAPopUpBranch()) return true;
        Debug.Log("Can't have PopUp as Hot Key as Hot Key");
        return false;
    }

    public void OnAwake()
    {
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        _uiPopUpEvents = new UIPopUpEvents();
        _notHomeScreenHotKey = _myBranch.MyBranchType != BranchType.HomeScreen;
        OnEnable();
    }
    
    public void OnEnable()
    {
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiControlsEvents.SubscribeToGameIsPaused(SaveIsPaused);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoActivePopUps);
        UIInput.HotKeyActivated += CheckHotKeys;
    }

    public void OnDisable()
    {
        UIInput.HotKeyActivated -= CheckHotKeys;
    }

    public bool CheckHotKeys()
    {
        if (!Input.GetButtonDown(_hotKeyAxis)) return false;
        if (_myBranch.CanvasIsEnabled || _gameIsPaused || !_noActivePopUps) return false;
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
