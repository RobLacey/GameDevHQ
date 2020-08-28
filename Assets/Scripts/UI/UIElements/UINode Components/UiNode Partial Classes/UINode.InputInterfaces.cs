using UnityEngine;
using UnityEngine.EventSystems;

public partial class UINode
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsDisabled || _allowKeys) return;
        _pointerOver = true;
        _uiActions._whenPointerOver?.Invoke(true);
        _navigation.PointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsDisabled || _allowKeys) return;
        _pointerOver = false;
        _uiActions._whenPointerOver?.Invoke(false);
        _navigation.PointerExit(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsDisabled || _allowKeys) return;
        PressedActions();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (NotActiveSlider) return;
        TurnNodeOnOff();
    }

    public void OnMove(AxisEventData eventData)
    {
        _uiActions._whenPointerOver?.Invoke(false);
        _navigation.KeyBoardOrController(eventData);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (IsDisabled) return;
        if (_buttonFunction == ButtonFunction.HoverToActivate) return;

        if (AmSlider) //TODO Need to check this still works properly
        {
            AmSlider.interactable = IsSelected;
        }
        PressedActions();
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        if(!_allowKeys) return;
        _uiActions._whenPointerOver?.Invoke(true);
    }
}