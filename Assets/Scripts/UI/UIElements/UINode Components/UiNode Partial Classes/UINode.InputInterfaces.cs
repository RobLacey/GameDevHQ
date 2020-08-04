using UnityEngine.EventSystems;

public partial class UINode
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsDisabled || _allowKeys) return;
        _pointerOver = true;
        TriggerEnterEvent();
        _navigation.PointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsDisabled || _allowKeys) return;
        _pointerOver = false;
        TriggerExitEvent();
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
        InvokeClickEvents();
        TurnNodeOnOff();
    }

    public void OnMove(AxisEventData eventData)
    {
        _navigation.KeyBoardOrController(eventData);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (IsDisabled) return;
        if (_buttonFunction == ButtonFunction.HoverToActivate) return;

        if (AmSlider) //TODO Need to check this still works properly
        {
            AmSlider.interactable = IsSelected;
            if (!IsSelected) InvokeClickEvents();
        }
        PressedActions();
    }
}