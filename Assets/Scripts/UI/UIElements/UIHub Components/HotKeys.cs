using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class HotKeys
{
    [SerializeField] 
    private HotKey _hotKeyInput;
    [SerializeField] 
    private UIBranch _myBranch;
    
    //Variables
    private UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private UIDataEvents _uiDataEvents = new UIDataEvents();
    private bool _hasParentNode;
    private bool _notHomeScreenHotKey;
    private bool _gameIsPaused;
    private bool _noActivePopUps = true;
    private INode _parentNode;
    private InputScheme _inputScheme;
    
    //Properties
    private void SaveIsPaused(bool isPaused) => _gameIsPaused = isPaused;
    private void SaveNoActivePopUps(bool noaActivePopUps) => _noActivePopUps = noaActivePopUps;

    //Events
    public static event Action FromHotKey;
    
    public void OnAwake(InputScheme inputScheme)
    {
        _inputScheme = inputScheme;
        _notHomeScreenHotKey = !_myBranch.IsHomeScreenBranch();
        IsAllowedType();
        OnEnable();
    }

    private void IsAllowedType()
    {
        if (_myBranch.IsAPopUpBranch())
            throw new Exception("Can't have a PopUp as a Hot Key");
        if (_myBranch.IsPauseMenuBranch())
            throw new Exception("Can't have Pause as a Hot Key");
        if (_myBranch.IsInternalBranch())
            throw new Exception("Can't have an Internal Branch as a Hot Key");
    }

    public void OnEnable()
    {
        _uiDataEvents.SubscribeToGameIsPaused(SaveIsPaused);
        _uiControlsEvents.SubscribeHotKeyActivation(CheckHotKeys);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoActivePopUps);
    }

    public bool CheckHotKeys()
    {
        if (!_inputScheme.HotKeyChecker(_hotKeyInput)) return false;
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
        StartThisHotKeyBranch();
        SetHotKeyAsSelectedActions();
    }

    private void GetParentNode()
    {
        var branchesNodes = _myBranch.MyParentBranch.ThisGroupsUiNodes;
        _parentNode = branchesNodes.First(node => _myBranch == node.HasChildBranch);
        _hasParentNode = true;
    }
    
    private void StartThisHotKeyBranch()
    {
        EnsureAlwaysReturnToHomeScreen();
        _myBranch.MoveToThisBranch();
    }

    private void SetHotKeyAsSelectedActions()
    {
        _parentNode.ThisNodeIsSelected();
        _parentNode.ThisNodeIsHighLighted();
        _parentNode.SetNodeAsSelected_NoEffects();
    }

    private void EnsureAlwaysReturnToHomeScreen()
    {
        if (_notHomeScreenHotKey)
             FromHotKey?.Invoke();  //Ensures back to home is used on cancel
    }
}
