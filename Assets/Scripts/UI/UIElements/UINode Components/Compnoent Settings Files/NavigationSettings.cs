using System;
using NaughtyAttributes;
using UnityEngine;

public interface INavigationSettings : IComponentSettings
{
    UIBranch ChildBranch { get; }
    NavigationType NavType { get; }
    UINode Up { get; }
    UINode Down { get; }
    UINode Left { get; }
    UINode Right { get; }
}

[Serializable]
public class NavigationSettings :INavigationSettings
{
    [SerializeField] 
    [AllowNesting] [Label("Move To When Clicked")] [HideIf("CantNavigate")] private UIBranch _childBranch;
    [SerializeField] 
    private NavigationType _setNavigation = NavigationType.UpAndDown;
    [SerializeField] 
    [AllowNesting] [ShowIf("UpDownNav")] private UINode _up;
    [SerializeField] 
    [AllowNesting] [ShowIf("UpDownNav")] private UINode _down;
    [SerializeField] 
    [AllowNesting] [ShowIf("RightLeftNav")] private UINode _left;
    [SerializeField] 
    [AllowNesting] [ShowIf("RightLeftNav")] private UINode _right;

    //Editor Scripts
    public bool CantNavigate { get; set; }

    public bool UpDownNav() 
        => _setNavigation == NavigationType.UpAndDown || _setNavigation == NavigationType.AllDirections;

    public bool RightLeftNav() 
        => _setNavigation == NavigationType.RightAndLeft || _setNavigation == NavigationType.AllDirections;

    public UIBranch ChildBranch => _childBranch;
    public NavigationType NavType => _setNavigation;
    public UINode Up => _up;
    public UINode Down => _down;
    public UINode Left => _left;
    public UINode Right => _right;
    public UINavigation Instance { get; set; }

    public NodeFunctionBase SetUp(UiActions uiActions, Setting functions)
    {
        if (uiActions.Node.IsToggleGroup || uiActions.Node.IsToggleNotLinked)
            _childBranch = null;
        
        if ((functions & Setting.NavigationAndOnClick) != 0)
        {
            Instance = new UINavigation(this, uiActions);
            return Instance;
        }
        return  new NullFunction();
    }
}
