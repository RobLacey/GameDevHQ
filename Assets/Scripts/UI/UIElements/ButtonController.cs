using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IEndDragHandler, IPointerEnterHandler, IPointerDownHandler,
                                               IPointerUpHandler, ISelectHandler, ISubmitHandler, IPointerExitHandler,
                                               IMoveHandler, IBeginDragHandler, ICancelHandler
{
    [SerializeField] ButtonMaster _childController;
    [SerializeField] ButtonMaster _parentController;
    [SerializeField] bool _justDisplayData;
    [SerializeField] ButtonColour _colours;
    [SerializeField] public ButtonAudio _audio;
    [SerializeField] ButtonAccessories _accessories;
    [SerializeField] ButtonSize _buttonSize;

    //Variables
    bool _selected = false;
    UICancelStopper _UICancelStopper;
    ButtonMaster _masterController;

    public Canvas MyCanvas { get; set; }

    private void Awake()//color
    {
        _masterController = GetComponentInParent<ButtonMaster>();
        MyCanvas = _masterController.MyCanvas;
        _UICancelStopper = FindObjectOfType<UICancelStopper>();
        _colours.OnAwake();
        _audio.OnAwake();
        _accessories.OnAwake();
        _buttonSize.OnAwake(transform);
    }


    public void OnPointerEnter(PointerEventData eventData) //Mouse highlight
    {
        if (eventData.pointerDrag) return; //Enables drag on slider to have pressed colour
        _audio.Play(UIEventTypes.Highlighted);
        SetHighlighted();
    }

    public void OnSelect(BaseEventData eventData) //KB/Ctrl highlight
    {
        SetHighlighted();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag) return; //Enables drag on slider to have pressed colour
        NotHighlighted();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_selected)
        {
            _audio.Play(UIEventTypes.Pressed);
        }
        _colours.SetColour(UIEventTypes.Pressed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SwitchUIDisplay();
    }

    public void OnSubmit(BaseEventData eventData) //KB/Ctrl
    {
        _audio.Play(UIEventTypes.Pressed);
        SwitchUIDisplay();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _audio.Play(UIEventTypes.Pressed);
        _colours.SetColour(UIEventTypes.Pressed);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _audio.Play(UIEventTypes.Pressed);
        _colours.SetColour(UIEventTypes.Highlighted);
    }

    public void OnMove(AxisEventData eventData)
    {
        if (_justDisplayData) return;
        if (eventData.selectedObject.GetComponent<Slider>())
        {
            if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
            {
                _audio.Play(UIEventTypes.Pressed);
                _colours.SetColour(UIEventTypes.Pressed);
            }

            if (eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down)
            {
                _audio.Play(UIEventTypes.Highlighted);
            }
        }
        else
        {
            _audio.Play(UIEventTypes.Highlighted);
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (_parentController)
        {
            Cancel();
        }
    }

    public void SetHighlight_Nothing_Selected()//color
    {
        if (_childController)
        {
            _childController.MyCanvas.enabled = false;
        }
        _selected = false;
        SetHighlighted();
    }

    public void SetHighlighted()
    {
        _UICancelStopper.SetLastUIObject(gameObject);
        _colours.SetColour(UIEventTypes.Highlighted);
        _masterController.OnHoverOver(gameObject);
        _accessories.ActivatePointer(true);
        _buttonSize.ScaleUp(transform);
    }

    private void SwitchUIDisplay()
    {
        _UICancelStopper.SetLastUIObject(gameObject);
        _masterController.ClearOtherSelections(gameObject);

        if (_childController)
        {
            if (_childController.MyCanvas.enabled == true)
            {
                DisableUIElement();
            }
            else
            {
                ActivateUIElement();
            }
        }
        else
        {
            _colours.SetColour(UIEventTypes.Pressed);
        }
    }

    private void DisableUIElement()
    {
        _childController.DefaultStartPosition.Cancel();
    }

    private void ActivateUIElement()
    {
        _colours.SetColour(UIEventTypes.Selected);
        _childController.MyCanvas.enabled = true;
        _selected = true;

        if (_childController)
        {
            _UICancelStopper.TrackOpenMenus(_masterController);
            _UICancelStopper.TrackOpenMenus(_childController);
            _masterController.MoveToChildLevel(this);
            _childController.FirstSelected();
        }
    }

    public void NotHighlighted() //color
    {
        if (_selected)
        {
            _colours.SetColour(UIEventTypes.Selected);

        }
        else
        {
            _colours.SetColour(UIEventTypes.Normal);
        }
        _buttonSize.ScaleDown(transform);
        _accessories.ActivatePointer(false);
    }

    public void ClearUISelection()
    {
        _selected = false;
        _colours.SetColour(UIEventTypes.Normal);

        if (_childController)
        {
            _childController.MyCanvas.enabled = false;
        }
        _accessories.ActivatePointer(false);
    }

    public void Cancel()
    {
        _UICancelStopper.TrackOpenMenus(_masterController);

        _audio.Play(UIEventTypes.Cancelled);

        if (_parentController)
        {
            _masterController.MoveToParentLevel(this);
            _parentController.FirstSelected();
        }    
    }
}


