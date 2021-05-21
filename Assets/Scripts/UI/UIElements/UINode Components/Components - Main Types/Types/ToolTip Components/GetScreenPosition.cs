using System;
using EZ.Inject;
using EZ.Service;
using UIElements;
using UnityEngine;
using UnityEngine.UI;

public interface IGetScreenPosition : IMono
{
    void SetExactPosition(bool isKeyboard);
    void LateStartSetUp();
}

public interface ITooltipCalcsData : IParameters
{
    float SafeZone { get; }
}

public class GetScreenPosition : IGetScreenPosition, IServiceUser, ITooltipCalcsData
{
    public GetScreenPosition(IToolTipData uiTooltip)
    {
        _tooltip = uiTooltip;
        _listOfTooltips = _tooltip.ListOfTooltips;
        _toolTipsRects = _tooltip.ToolTipsRects;
        _myCorners = _tooltip.MyCorners;
        _uiCamera = _tooltip.UiCamera;
        _parentRectTransform = _tooltip.ParentRectTransform;
    }

    private readonly IToolTipData _tooltip;
    private IToolTipCalcs _toolTipCalcs;
    private readonly LayoutGroup[] _listOfTooltips;
    private readonly RectTransform[] _toolTipsRects;
    private readonly Vector3[] _myCorners;
    private readonly RectTransform _parentRectTransform;
    private RectTransform _mainCanvasRectTransform;
    private readonly Camera _uiCamera;
    private InputScheme _inputScheme;
    private IDataHub _myDataHub;

    //Properties
    public float SafeZone => Scheme.ScreenSafeZone;
    private ToolTipScheme Scheme => _tooltip.Scheme;

    //Properties
    private Vector2 KeyboardPadding => new Vector2(Scheme.KeyboardXPadding, Scheme.KeyboardYPadding);
    private Vector2 MousePadding => new Vector2(Scheme.MouseXPadding, Scheme.MouseYPadding);

    //Main
    public void OnAwake() => _toolTipCalcs = EZInject.Class.WithParams<IToolTipCalcs>(this);

    public void OnEnable()
    {
        UseEZServiceLocator();
        _toolTipCalcs.OnEnable();
        LateStartSetUp();
    }

    public void UseEZServiceLocator()
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    public void LateStartSetUp()
    {
        if(_myDataHub.IsNull()) return;
        
        _mainCanvasRectTransform = _myDataHub.MainCanvasRect;
        _toolTipCalcs.OnStart();
    }

    public void OnDisable()
    {
        _inputScheme = null;
        _myDataHub = null;
        _toolTipCalcs.OnDisable();
    }

    public void OnDestroy()
    {
        _toolTipCalcs.OnDisable();
        _inputScheme = null;
        _myDataHub = null;
        _toolTipCalcs = null;
    }

    public void OnStart()
    {
        LateStartSetUp();
        _toolTipCalcs.OnStart();
    }

    public void SetExactPosition(bool isKeyboard)
    {
        var index = _tooltip.CurrentToolTipIndex;
        
        var toolTipSize = new Vector2(_listOfTooltips[index].preferredWidth
                                      , _listOfTooltips[index].preferredHeight);

        var toolTipAnchorPos  = isKeyboard ? Scheme.KeyboardPosition : Scheme.ToolTipPosition;

        var tooTipType = isKeyboard ? Scheme.ToolTipTypeKeys : Scheme.ToolTipTypeMouse;
        
        var toolTipPos = GetToolTipsScreenPosition(isKeyboard, tooTipType);

        (_toolTipsRects[index].anchoredPosition, _toolTipsRects[index].pivot)
            = _toolTipCalcs.CalculatePosition(toolTipPos, toolTipSize, toolTipAnchorPos);
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
        => ReturnScreenPosition(_inputScheme.GetMouseOrVcPosition()) + MousePadding;

    private Vector3 SetKeyboardTooltipPosition()
    {
        var toolTipPosition = Vector3.zero;
        toolTipPosition = PositionBasedOnSettings(toolTipPosition);
        toolTipPosition += _parentRectTransform.transform.position;
        
        return ReturnScreenPosition(toolTipPosition) + KeyboardPadding;
    }

    private Vector3 PositionBasedOnSettings(Vector3 position)
    {
        switch (Scheme.PositionToUse)
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
            (_mainCanvasRectTransform, screenPosition, _uiCamera, out var toolTipPos);
        return toolTipPos;
    }
}

