
using UnityEngine;

public interface IMoveVirtualCursor
{
    void OnEnable();

    void Move(ICursorSettings cursor);
}

public class MoveVirtualCursor : IMoveVirtualCursor, IEServUser
{
    //Variables
    private Vector2 _newCursorPos = Vector2.zero;
    private int _screenLeft;
    private int _screenRight;
    private int _screenBottom;
    private int _screenTop;
    private InputScheme _inputScheme;
    private IHub _hub;

    //Main
    public void OnEnable() => UseEServLocator();

    public void UseEServLocator()
    {
        _inputScheme = EServ.Locator.Get<IInput>(this).ReturnScheme;
        _hub = EServ.Locator.Get<IHub>(this);
        if(_hub.IsNotNull())
        {
            SetScreenSize(_hub.MainCanvasRect);
        }    
    }

    private void SetScreenSize(RectTransform hubMainCanvas)
    {
        var sizeDelta = hubMainCanvas.sizeDelta;
        
        _screenLeft = Mathf.RoundToInt(sizeDelta.x * -0.5f);
        _screenRight = Mathf.RoundToInt(sizeDelta.x  * 0.5f);
        _screenBottom = Mathf.RoundToInt(sizeDelta.y * -0.5f);
        _screenTop = Mathf.RoundToInt(sizeDelta.y  * 0.5f);
    }

    public void Move(ICursorSettings cursor)
    {
        _newCursorPos.x = _inputScheme.VcHorizontal() * cursor.Speed;
        _newCursorPos.y = _inputScheme.VcVertical() * cursor.Speed;
        
        CalculateNewPosition(cursor.CursorRect);
        _inputScheme.SetVirtualCursorPosition(cursor.Position);
    }

    private void CalculateNewPosition(RectTransform cursorRect)
    {
        var temp = cursorRect.anchoredPosition + _newCursorPos;
          temp.x = Mathf.Clamp(temp.x, _screenLeft, _screenRight);
          temp.y = Mathf.Clamp(temp.y, _screenBottom, _screenTop);
          cursorRect.anchoredPosition = temp;
    }
}
