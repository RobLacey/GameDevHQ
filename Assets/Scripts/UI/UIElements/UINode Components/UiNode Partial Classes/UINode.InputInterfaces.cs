using UnityEngine.EventSystems;

public partial class UINode : IPointerEnterHandler, IPointerDownHandler,
                              IMoveHandler, IPointerUpHandler, ISubmitHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (DontAllowPointerEvent(eventData)) return;
        HandleOnEnter();
    }

    public void HandleOnEnter()
    {
        if(_disable.IsDisabled) return;

        if (IsHoverToActivate & !IsSelected)
        {
            _pointerOver = false;
            _nodeBase.TurnNodeOnOff();
        }
        else
        {
            _pointerOver = true;
            SetAsHighlighted();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (DontAllowPointerEvent(eventData)) return;
        if (IsHoverToActivate && _closeHoverOnExit)
        {
            _nodeBase.TurnNodeOnOff();
        }
        _pointerOver = false;
        SetNotHighlighted();
    }

    private bool DontAllowPointerEvent(PointerEventData eventData) => _allowKeys || eventData.pointerDrag;

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

    public void OnMove(AxisEventData eventData)
    {
        DoMove(eventData.moveDir);
    }

    public void CheckIfMoveAllowed(MoveDirection moveDirection)
    {
        if(!_allowKeys) return;
        if(!MyBranch.CanvasIsEnabled)return;
        
        if (_disable.IsDisabled)
        {
            DoMove(moveDirection);
        }
        else
        {
            HandleOnEnter();
        }
    }
    
    private void DoMove(MoveDirection moveDirection)
    {
        if(!_allowKeys) return;
        _uiActions._onMove?.Invoke(moveDirection);
        
        if (AmSlider && IsSelected)
        {
            _navigation.Instance.HandleAsSlider();
        }
        else
        {
            _pointerOver = false;
            _navigation.Instance.ProcessMoves();
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if(!_allowKeys) return;
        _pointerOver = false;
        if (_disable.IsDisabled || _buttonFunction == ButtonFunction.HoverToActivate) return;
        SetSliderUpForInteraction();
        PressedActions();
    }
    
    private void SetSliderUpForInteraction()
    {
        if (AmSlider) //TODO Need to check this still works properly
        {
            AmSlider.interactable = IsSelected;
        }
    }
}
