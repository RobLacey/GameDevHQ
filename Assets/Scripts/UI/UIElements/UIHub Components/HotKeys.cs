using System;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class HotKeys
{
    [SerializeField] 
    [InputAxis] [AllowNesting] private string _hotKeyAxis;
    
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
    private UINode _parentNode;

    //Properties
    private void SaveIsPaused(bool isPaused) => _gameIsPaused = isPaused;
    private void SaveNoActivePopUps(bool noaActivePopUps) => _noActivePopUps = noaActivePopUps;

    //Events
    public static event Action FromHotKey;
    
    public void OnAwake()
    {
        _notHomeScreenHotKey = _myBranch.MyBranchType != BranchType.HomeScreen;
        IsAllowedType();
        OnEnable();
    }

    private void IsAllowedType()
    {
        if (_myBranch.IsAPopUpBranch())
            throw new Exception("Can't have a PopUp as a Hot Key");
        if (_myBranch.IsPauseMenuBranch())
            throw new Exception("Can't have Pause as a Hot Key");
        if (_myBranch.MyBranchType == BranchType.Internal)
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
        StartThisHotKeyBranch();
        SetHotKeyAsSelectedActions();
    }

    private void GetParentNode()
    {
        if(_parentNode != null) return;
        foreach (UINode parentNode in _myBranch.MyParentBranch.ThisGroupsUiNodes)
        {
            if (parentNode.HasChildBranch != _myBranch) continue;
            _hasParentNode = true;
            _parentNode = parentNode;
            break;
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
        _myBranch.DefaultStartOnThisNode.Audio.Play(UIEventTypes.Selected);
    }

    private void EnsureAlwaysReturnToHomeScreen()
    {
        if (_notHomeScreenHotKey)
             FromHotKey?.Invoke();  //Ensures back to home is used on cancel
    }
}
