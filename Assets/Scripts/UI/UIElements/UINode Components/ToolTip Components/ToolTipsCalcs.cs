﻿using UnityEngine;
using static UnityEngine.Mathf;

public class ToolTipsCalcs
{
    private readonly float _canvasWidth, _canvasHeight;
    private Vector3 _toolTipPosition, _tooltipSize;
    private Vector2 _newPivot, _parentNodePosition, _offset;
    
    public ToolTipsCalcs(RectTransform mainCanvas, float safeZone)
    {
        var rect = mainCanvas.rect;
        _canvasWidth = (rect.width / 2) - safeZone;
        _canvasHeight = (rect.height / 2) - safeZone;
    }
    
    public (Vector3 _toolTipData, Vector2 _newPivot) CalculatePosition
        (Vector3 tooltipPos, Vector3 offset, Vector3 toolTipSize, ToolTipAnchor toolTipAnchor)
    {
        SetVariables(tooltipPos, toolTipSize);
        (_toolTipPosition, _newPivot) = CalculateAnchorPosition(toolTipAnchor);
        _toolTipPosition = new Vector2(offset.x + _toolTipPosition.x, offset.y + _toolTipPosition.y);
        return (_toolTipPosition, _newPivot);
    }

    private void SetVariables(Vector3 tooltipPos, Vector3 tooltipSize)
    {
        _toolTipPosition = tooltipPos;
        _tooltipSize = tooltipSize;
    }

    private (Vector2 pos, Vector2 pivot) CalculateAnchorPosition(ToolTipAnchor toolTipAnchor)
    {
        switch (toolTipAnchor)
        {
            case ToolTipAnchor.Centre:
                return (new Vector3(MiddleX(), MiddleY()), new Vector2(0.5f, 0.5f));
            case ToolTipAnchor.MiddleLeft:
                return (new Vector3(LeftX(), MiddleY()), new Vector2(1f, 0.5f));
            case ToolTipAnchor.MiddleRight:
                return (new Vector3(RightX(), MiddleY()), new Vector2(0f, 0.5f));
            case ToolTipAnchor.MiddleTop:
                return (new Vector3(MiddleX(), TopY()), new Vector2(0.5f, 0f));
            case ToolTipAnchor.MiddleBottom:
                return (new Vector3(MiddleX(), BottomY()), new Vector2(0.5f, 1f));
            case ToolTipAnchor.TopLeft:
                return (new Vector3(LeftX(), TopY()), new Vector2(1f, 0f));
            case ToolTipAnchor.TopRight:
                return (new Vector3(RightX(), TopY()), new Vector2(0f, 0f));
            case ToolTipAnchor.BottomLeft:
                return (new Vector3(LeftX(), BottomY()), new Vector2(1f, 1f));
            case ToolTipAnchor.BottomRight:
                return (new Vector3(RightX(), BottomY()), new Vector2(0f, 1f));
            default:
                Debug.Log("No Match Found");
                return (_toolTipPosition, _newPivot);
        }
    }

    private float RightX() => Clamp(_toolTipPosition.x, (-_canvasWidth), (_canvasWidth) - _tooltipSize.x);

    private float BottomY() => Clamp(_toolTipPosition.y, _tooltipSize.y + (-_canvasHeight), _canvasHeight);

    private float TopY() => Clamp(_toolTipPosition.y, (-_canvasHeight), (_canvasHeight) - _tooltipSize.y);

    private float MiddleX() 
        => Clamp(_toolTipPosition.x, (-_canvasWidth) + (_tooltipSize.x / 2), (_canvasWidth) - (_tooltipSize.x / 2));

    private float MiddleY() 
        => Clamp(_toolTipPosition.y, (_tooltipSize.y / 2) + (-_canvasHeight), _canvasHeight - (_tooltipSize.y / 2));

    private float LeftX() => Clamp(_toolTipPosition.x, (-_canvasWidth) + _tooltipSize.x, (_canvasWidth));
}