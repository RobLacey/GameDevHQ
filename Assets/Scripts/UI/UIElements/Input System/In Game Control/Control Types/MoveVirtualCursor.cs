
using UnityEngine;

public interface IMoveVirtualCursor
{
    void Move(VirtualCursor cursor);
}

public class MoveVirtualCursor : IMoveVirtualCursor
{
    private Vector2 _newCursorPos = Vector2.zero;
    private readonly int _screenLeft = -Screen.width / 2;
    private readonly int _screenRight = Screen.width / 2;
    private readonly int _screenBottom = -Screen.height / 2;
    private readonly int _screenTop = Screen.height / 2;

    public void Move(VirtualCursor cursor)
    {
        _newCursorPos.x = cursor.Scheme.VcHorizontal() * cursor.Speed;
        _newCursorPos.y = cursor.Scheme.VcVertical() * cursor.Speed;
        
        CalculateNewPosition(cursor.CursorRect);
        cursor.Scheme.SetVirtualCursorPosition(cursor.Position);
    }

    private void CalculateNewPosition(RectTransform cursorCursorRect)
    {
        var temp = cursorCursorRect.anchoredPosition + _newCursorPos;
        temp.x = Mathf.Clamp(temp.x, _screenLeft, _screenRight);
        temp.y = Mathf.Clamp(temp.y, _screenBottom, _screenTop);
        cursorCursorRect.anchoredPosition = temp;
    }

}
