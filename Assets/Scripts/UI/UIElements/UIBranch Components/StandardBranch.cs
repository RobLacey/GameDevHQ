
using System;
using UnityEngine;

public class StandardBranch : Branch, IBranch
{
    public StandardBranch(Canvas canvas, CanvasGroup canvasGroup, ScreenType screenType, UIBranch branch)
    {
        _screenType = screenType;
        _myCanvas = canvas;
        _myBranch = branch;
        _myCanvasGroup = canvasGroup;
        _uiDataEvents.SubscribeToOnHomeScreen(SaveIfOnHomeScreen);
    }
    
    private ScreenType _screenType;
    private UIBranch _myBranch;
    private readonly Canvas _myCanvas;
    private readonly CanvasGroup _myCanvasGroup;
    private bool _onHomeScreen = true; 
    private UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly bool _isHomeScreenBranch = false;

    private void SaveIfOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;

    public void SetUpStartUpBranch(UIBranch startBranch, IsActive inMenu)
    {
        
    }

    public void ActivateBranch()
    {
        _myCanvasGroup.blocksRaycasts = true;
        _myCanvas.enabled = true;
    }

    public void ClearBranch(UIBranch ignoreThisBranch = null)
    {
        if (ignoreThisBranch == _myBranch || !_myBranch.CanvasIsEnabled) return;
        Debug.Log("Standard clear");
        _myCanvas.enabled = false;
        _myCanvasGroup.blocksRaycasts = false;
    }

    public void CanClearOrRestoreScreen()
    {
        if (_screenType == ScreenType.FullScreen)
        {
            if (_onHomeScreen)
            {
                InvokeHomeScreen(_isHomeScreenBranch);
            }
            else
            {
                InvokeDoClearScreen(_myBranch);
            }
        }
    }
}
