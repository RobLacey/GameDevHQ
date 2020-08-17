using System;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class HotKeys
{
    [SerializeField] 
    [InputAxis] [AllowNesting] private string _hotKeyAxis;
    [SerializeField] 
    [ValidateInput("IsAllowedType", "Can't have PopUp as HotKey as HotKey")] 
    private UIBranch _myBranch;
    
    //Variables
    private UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
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
    
    //EditorScript
    private bool IsAllowedType()
    {
        if (!_myBranch.IsAPopUpBranch()) return true;
        Debug.Log("Can't have PopUp as Hot Key as Hot Key");
        return false;
    }

    public void OnAwake()
    {
        _notHomeScreenHotKey = _myBranch.MyBranchType != BranchType.HomeScreen;
        OnEnable();
    }
    
    public void OnEnable()
    {
        _uiControlsEvents.SubscribeToGameIsPaused(SaveIsPaused);
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
        _myBranch.DefaultStartPosition.Audio.Play(UIEventTypes.Selected);
    }

    private void EnsureAlwaysReturnToHomeScreen()
    {
        if (_notHomeScreenHotKey)
             FromHotKey?.Invoke();  //Ensures back to home is used on cancel
    }
}
