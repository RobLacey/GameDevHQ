
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
        _calculation = new ToolTipsCalcs(_tooltip.MainCanvas, _scheme.ScreenSafeZone);
    }

    private readonly IToolTipData _tooltip;
    private readonly ToolTipsCalcs _calculation;
    private readonly ToolTipScheme _scheme;
    private readonly LayoutGroup[] _listOfTooltips;
    private readonly RectTransform[] _toolTipsRects;
    private readonly Vector3[] _myCorners;
    
    //Properties
    private Vector2 KeyboardPadding => new Vector2(_scheme.KeyboardXPadding, _scheme.KeyboardYPadding);
    private Vector2 MousePadding => new Vector2(_scheme.MouseXPadding, _scheme.MouseYPadding);


    public void SetExactPosition(bool isKeyboard)
    {
        var index = _tooltip.CurrentToolTipIndex;
        
        var toolTipSize = new Vector2(_listOfTooltips[index].preferredWidth
                                      , _listOfTooltips[index].preferredHeight);

        var toolTipAnchorPos 
            = !isKeyboard ? _scheme.ToolTipPosition : _scheme.KeyboardPosition;
        
        var toolTipPos = GetToolTipsScreenPosition(isKeyboard, _scheme.ToolTipType);

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
            case TooltipType.Fixed:
                return SetFixedToolTipPosition();
        }
        return Vector3.zero;
    }

    private Vector3 SetFixedToolTipPosition() => ReturnScreenPosition(_scheme.GroupFixedPosition.position);

    private Vector3 SetMouseToolTipPosition() => ReturnScreenPosition(Input.mousePosition) + MousePadding;

    private Vector3 SetKeyboardTooltipPosition()
    {
        var tolTipPosition = Vector3.zero;
        tolTipPosition = PositionBasedOnSettings(tolTipPosition);
        return ReturnScreenPosition(tolTipPosition) + KeyboardPadding;
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
            case UseSide.GameObjectAsPosition:
                position = _scheme.FixedPosition.position;
                break;
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

