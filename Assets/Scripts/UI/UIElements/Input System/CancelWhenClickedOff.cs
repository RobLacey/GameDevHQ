
using UnityEngine.EventSystems;

public class CancelWhenClickedOff
{
    public static bool CanCancel(InputScheme scheme, bool onHomeScreen, bool allowKeys)
    {
        if (allowKeys || !onHomeScreen) return false;
        
        if (scheme.CanUseVirtualCursor == VirtualControl.Yes)
        {
            return VCCancel(scheme);
        }

        return MouseCancel(scheme);
    }
    
    private static bool VCCancel(InputScheme scheme)
    {
        if (!scheme.PressSelect()) return false;
        
        return !scheme.ReturnVirtualCursor.OverAnyObject
               && scheme.CanCancelWhenClickedOff != CancelClickLocation.Never;
    }

    private static bool MouseCancel(InputScheme scheme)
    {
        if (EventSystem.current.IsPointerOverGameObject())
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
