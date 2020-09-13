using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;

public class ToolTipsCalcs
{
    private readonly float _canvasWidth;
    private readonly float _canvasHeight;
    private float _preferredWidth;
    private float _preferredHeight;
    private Vector3 _toolTipData;
    
    public ToolTipsCalcs(RectTransform mainCanvas, float safeZone)
    {
        var rect = mainCanvas.rect;
        _canvasWidth = (rect.width / 2) - safeZone;
        _canvasHeight = (rect.height / 2) - safeZone;
    }
    
    public (Vector3 pos, Vector2 pivot) CalcCentreClamp(Vector3 tooltipPos, ToolTipAnchor toolTipAnchor, LayoutGroup layoutGroup)
    {
        _preferredWidth = layoutGroup.preferredWidth;
        _preferredHeight = layoutGroup.preferredHeight;
        _toolTipData = tooltipPos;

        switch (toolTipAnchor)
        {
            case ToolTipAnchor.Centre:
                return (new Vector3(MiddleX(), MiddleY(), tooltipPos.z), new Vector2(0.5f, 0.5f));
            case ToolTipAnchor.MiddleLeft:
                return (new Vector3(LeftX(), MiddleY(), tooltipPos.z), new Vector2(1f, 0.5f));
            case ToolTipAnchor.MiddleRight:
                return (new Vector3(RightX(), MiddleY(), tooltipPos.z), new Vector2(0f, 0.5f));
            case ToolTipAnchor.MiddleTop:
                return (new Vector3(MiddleX(), TopY(), tooltipPos.z), new Vector2(0.5f, 0f));
            case ToolTipAnchor.MiddleBottom:
                return (new Vector3(MiddleX(), BottomY(), tooltipPos.z), new Vector2(0.5f, 1f));
            case ToolTipAnchor.TopLeft:
                return (new Vector3(LeftX(), TopY(), tooltipPos.z), new Vector2(1f, 0f));
            case ToolTipAnchor.TopRight:
                return (new Vector3(RightX(), TopY(), tooltipPos.z), new Vector2(0f, 0f));
            case ToolTipAnchor.BottomLeft:
                return (new Vector3(LeftX(), BottomY(), tooltipPos.z), new Vector2(1f, 1f));
            case ToolTipAnchor.BottomRight:
                return (new Vector3(RightX(), BottomY(), tooltipPos.z), new Vector2(0f, 1f));
            default:
                Debug.Log("No Match Found");
                return (tooltipPos, Vector2.zero);
        }
    }

    private float RightX() => Clamp(_toolTipData.x, (-_canvasWidth), (_canvasWidth) - _preferredWidth);

    private float BottomY() => Clamp(_toolTipData.y, _preferredHeight + (-_canvasHeight), _canvasHeight);

    private float TopY() => Clamp(_toolTipData.y, (-_canvasHeight), (_canvasHeight) - _preferredHeight);

    private float MiddleX() 
        => Clamp(_toolTipData.x, (-_canvasWidth) + (_preferredWidth / 2), (_canvasWidth) - (_preferredWidth / 2));

    private float MiddleY() 
        => Clamp(_toolTipData.y, (_preferredHeight / 2) + (-_canvasHeight), _canvasHeight - (_preferredHeight / 2));

    private float LeftX() => Clamp(_toolTipData.x, (-_canvasWidth) + _preferredWidth, (_canvasWidth));
}
