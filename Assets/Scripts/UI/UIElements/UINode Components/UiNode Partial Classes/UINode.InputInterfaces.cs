using UnityEngine.EventSystems;

public partial class UINode
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DontAllowPointerEvent(eventData)) return;
        
        if (_buttonFunction == ButtonFunction.HoverToActivate & !IsSelected)
        {
            PressedActions();
        }
        else
        {
            SetAsHighlighted();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (DontAllowPointerEvent(eventData)) return;
        SetNotHighlighted();
    }

    private bool DontAllowPointerEvent(PointerEventData eventData) => IsDisabled || eventData.pointerDrag;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (DontAllowPointerEvent(eventData)) return;
        PressedActions();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //if (NotActiveSlider) return;
        //TurnNodeOnOff();
    }

    public void OnMove(AxisEventData eventData) => DoMove(eventData.moveDir);

    public void CheckIfMoveAllowed(MoveDirection moveDirection)
    {
        if (IsDisabled)
        {
            DoMove(moveDirection);
        }
        else
        {
            OnPointerEnter(new PointerEventData(EventSystem.current));
        }
    }
    
    private void DoMove(MoveDirection moveDirection)
    {
        _uiActions._onMove?.Invoke(moveDirection);
        if (AmSlider && IsSelected)
        {
            _navigation.HandleAsSlider();
        }
        else
        {
            _navigation.ProcessMoves();
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (IsDisabled || _buttonFunction == ButtonFunction.HoverToActivate) return;
        SetSliderUpForInteraction();
        PressedActions();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(!_allowKeys) return;
        _uiActions._whenPointerOver?.Invoke(true);
    }

    private void SetSliderUpForInteraction()
    {
        if (AmSlider) //TODO Need to check this still works properly
        {
            AmSlider.interactable = IsSelected;
        }
    }
}
