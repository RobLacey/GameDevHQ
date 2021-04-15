using UnityEngine.EventSystems;

public class CancelWhenClickedOff
{
    public static bool CanCancel(InputScheme scheme, bool onHomeScreen, bool allowKeys, IVirtualCursor virtualCursor)
    {
        if (allowKeys || !onHomeScreen) return false;
        
        return scheme.CanUseVirtualCursor ? VCCancel(scheme, virtualCursor) : MouseCancel(scheme);
    }
    
    private static bool VCCancel(InputScheme scheme, IVirtualCursor virtualCursor)
    {
        if (!scheme.PressSelect()) return false;

        return virtualCursor.OverAnyObject.IsNull() && scheme.CanCancelWhenClickedOff != CancelClickLocation.Never;
    }

    private static bool MouseCancel(InputScheme scheme)
    {
        if (EventSystem.current.IsPointerOverGameObject() || !scheme.AnyMouseClicked)
        {
            return false;
        }

        switch (scheme.CanCancelWhenClickedOff)
        {
            case CancelClickLocation.Never:
                return false;
            case CancelClickLocation.Left:
                return scheme.LeftMouseClicked;
            case CancelClickLocation.Right:
                return scheme.RightMouseClicked;
            case CancelClickLocation.Either: 
                return scheme.AnyMouseClicked;
            default:
                return false;
        }
    }
}
