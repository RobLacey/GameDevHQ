
using System;
using UnityEngine;
using UnityEngine.UI;

public interface IGetScreenPosition
{
    void SetExactPosition(bool isKeyboard);
}

public class GetScreenPosition : IGetScreenPosition
{
    public GetScreenPosition(IToolTipData uiTooltip)
    {
        _tooltip = uiTooltip;
        _scheme = _tooltip.Scheme;
        _listOfTooltips = _tooltip.ListOfTooltips;
        _toolTipsRects = _tooltip.ToolTipsRects;
        _myCorners = _tooltip.MyCorners;
        _parentRectTransform = _tooltip.ParentRectTransform;
        _calculation = new ToolTipsCalcs(_tooltip.MainCanvas, _scheme.ScreenSafeZone);
    }

    private readonly IToolTipData _tooltip;
    private readonly ToolTipsCalcs _calculation;
    private readonly ToolTipScheme _scheme;
    private readonly LayoutGroup[] _listOfTooltips;
    private readonly RectTransform[] _toolTipsRects;
    private readonly Vector3[] _myCorners;
    private readonly RectTransform _parentRectTransform;
    
    //Properties
    private Vector2 KeyboardPadding => new Vector2(_scheme.KeyboardXPadding, _scheme.KeyboardYPadding);
    private Vector2 MousePadding => new Vector2(_scheme.MouseXPadding, _scheme.MouseYPadding);

    public void SetExactPosition(bool isKeyboard)
    {
        var index = _tooltip.CurrentToolTipIndex;
        
        var toolTipSize = new Vector2(_listOfTooltips[index].preferredWidth
                                      , _listOfTooltips[index].preferredHeight);

        var toolTipAnchorPos  = isKeyboard ? _scheme.KeyboardPosition : _scheme.ToolTipPosition;

        var tooTipType = isKeyboard ? _scheme.ToolTipTypeKeys : _scheme.ToolTipTypeMouse;
        
        var toolTipPos = GetToolTipsScreenPosition(isKeyboard, tooTipType);

        (_toolTipsRects[index].anchoredPosition, _toolTipsRects[index].pivot)
            = _calculation.CalculatePosition(toolTipPos, toolTipSize, toolTipAnchorPos);
    }

    private Vector3 GetToolTipsScreenPosition(bool isKeyboard, TooltipType toolTipType)
    {
        switch (toolTipType)
        {
            case TooltipType.Follow when isKeyboard:
                return SetKeyboardTooltipPosition();
            case TooltipType.Follow:
                return SetMouseToolTipPosition();
            case TooltipType.FixedPosition:
                return SetFixedToolTipPosition();
            default:
                throw new ArgumentOutOfRangeException(nameof(toolTipType), toolTipType, null);
        }
    }

    private Vector3 SetFixedToolTipPosition() => ReturnScreenPosition(_tooltip.FixedPosition.position);

    private Vector3 SetMouseToolTipPosition() 
        => ReturnScreenPosition(_tooltip.InputScheme.GetMouseOrVcPosition()) + MousePadding;

    private Vector3 SetKeyboardTooltipPosition()
    {
        var toolTipPosition = Vector3.zero;
        toolTipPosition = PositionBasedOnSettings(toolTipPosition);
        
        if (_scheme.Follow())
            toolTipPosition += _parentRectTransform.transform.position;
        
        return ReturnScreenPosition(toolTipPosition) + KeyboardPadding;
    }

    private Vector3 PositionBasedOnSettings(Vector3 position)
    {
        switch (_scheme.PositionToUse)
        {
            case UseSide.ToTheRightOf:
                position = _myCorners[3] + ((_myCorners[2] - _myCorners[3]) / 2);
                break;
            case UseSide.ToTheLeftOf:
                position = _myCorners[1] + ((_myCorners[0] - _myCorners[1]) / 2);
                break;
            case UseSide.ToTheTopOf:
                position = _myCorners[1] + ((_myCorners[2] - _myCorners[1]) / 2);
                break;
            case UseSide.ToTheBottomOf:
                position = _myCorners[0] + ((_myCorners[3] - _myCorners[0]) / 2);
                break;
            case UseSide.CentreOf:
                position = Vector3.zero;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return position;
    }

    private Vector2 ReturnScreenPosition(Vector3 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (_tooltip.MainCanvas, screenPosition, _tooltip.UiCamera, out var toolTipPos);
        return toolTipPos;
    }

}

