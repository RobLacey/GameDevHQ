
using System;
using UnityEngine;

public interface IBranch
{
    void SetUpStartUpBranch(UIBranch startBranch, IsActive inMenu);
    void ActivateBranch();
    void ClearBranch(UIBranch ignoreThisBranch = null);
    void CanClearOrRestoreScreen();
}

public class Branch
{
    public static event Action<bool> SetIsOnHomeScreen; // Subscribe To track if on Home Screen
    public static event Action<UIBranch> DoClearScreen; // Subscribe To track if on Home Screen

    protected void InvokeHomeScreen(bool onHome)
    {
        SetIsOnHomeScreen?.Invoke(onHome);
    }

    protected void InvokeDoClearScreen(UIBranch ignoreThisBranch)
    {
        DoClearScreen?.Invoke(ignoreThisBranch);
    }
}

public class HomeScreenBranch: Branch, IBranch
{
    public HomeScreenBranch(Canvas canvas, CanvasGroup canvasGroup, IsActive tweenOnHome, UIBranch branch)
    {
        _myCanvas = canvas;
        _myBranch = branch;
        _myCanvasGroup = canvasGroup;
        _tweenOnHome = tweenOnHome;
        _uiDataEvents.SubscribeToOnHomeScreen(SaveIfOnHomeScreen);
    }
    
    //private bool _onHomeScreen = true;
    private readonly Canvas _myCanvas;
    private readonly CanvasGroup _myCanvasGroup;
    private readonly IsActive _tweenOnHome;
    private readonly UIBranch _myBranch;
    private bool _onHomeScreen = true; 
    private UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly bool _isHomeScreenBranch = true;


    private void SaveIfOnHomeScreen(bool onHomeScreen)
    {
        if(onHomeScreen && _onHomeScreen) return;
        
        _onHomeScreen = onHomeScreen;
        
        if (onHomeScreen)
        {
            ResetHomeScreenBranch();
        }
        else
        {
            ClearBranch();
        }
    }
    
    public void SetUpStartUpBranch(UIBranch startBranch, IsActive inMenu)
    {
        if (startBranch == _myBranch)
        {
            _myBranch.DefaultStartPosition.ThisNodeIsHighLighted();
            _myBranch.DefaultStartPosition.ThisNodeIsSelected();
            _myBranch.SetAsActiveBranch();
            
            if (inMenu == IsActive.Yes)
            {
                _myBranch.MoveToThisBranch();
                return;
            }
        }
        _myBranch.MoveToThisBranchDontSetAsActive();
    }
    
    public void ActivateBranch()
    {
        _myCanvasGroup.blocksRaycasts = true;
        _myCanvas.enabled = true;
    }

    public void CanClearOrRestoreScreen()
    {
        InvokeHomeScreen(_isHomeScreenBranch);
    }

    private void ResetHomeScreenBranch()
    {
        _myBranch._tweenOnChange = _tweenOnHome == IsActive.Yes;
        if (_tweenOnHome == IsActive.Yes)
            _myBranch.ActivateInTweens();
        _myCanvas.enabled = true;
        _myCanvasGroup.blocksRaycasts = true;
    }

    public void ClearBranch(UIBranch ignoreThisBranch = null)
    {
        _myCanvasGroup.blocksRaycasts = false;
        _myCanvas.enabled = false;
    }
}


