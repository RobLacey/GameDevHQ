using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ColourLerp))]
public class ButtonController : MonoBehaviour, IEndDragHandler, IPointerEnterHandler, IPointerDownHandler,
                                                ISelectHandler, ISubmitHandler, IPointerExitHandler,
                                               IMoveHandler, IBeginDragHandler, ICancelHandler
{
    [SerializeField] ButtonMaster _childController;
    [SerializeField] bool _isCancelOrBackButton;
    [SerializeField] bool _justDisplayData;
    [SerializeField] ButtonColour _colours;
    [SerializeField] public ButtonAudio _audio;
    [SerializeField] ButtonAccessories _accessories;
    [SerializeField] ButtonSize _buttonSize;
    [SerializeField] InvertColours _invertColourCorrection;
    [SerializeField] Swapper _swapImageOrText;

    //Variables
    bool _selected = false;
    UICancelStopper _UICancelStopper;
    ButtonMaster _masterController;
    Slider _amSlider;

    public ButtonMaster MyParentController { get; set; }

    private void Awake()//color
    {
        if(TryGetComponent(out _amSlider))
        {
            _amSlider.interactable = false;
        }
        _colours.MyColourLerper = GetComponent<ColourLerp>();
        _masterController = GetComponentInParent<ButtonMaster>();
        _UICancelStopper = FindObjectOfType<UICancelStopper>();
        _colours.OnAwake();
        _audio.OnAwake();
        _accessories.OnAwake();
        _buttonSize.OnAwake(transform);
        _invertColourCorrection.OnAwake();
        _swapImageOrText.OnAwake();
        _audio.MyAudiosource = GetComponentInParent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData) //Mouse highlight
    {
        if (_justDisplayData) return;
        if (eventData.pointerDrag) return; //Enables drag on slider to have pressed colour
        _audio.Play(UIEventTypes.Highlighted);
        SetHighlighted();
    }

    public void OnSelect(BaseEventData eventData) //KB/Ctrl highlight
    {
        if (_justDisplayData) return;
        SetHighlighted();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_justDisplayData) return;
        if (eventData.pointerDrag) return; //Enables drag on slider to have pressed colour
        HandleSliderOff();
        NotHighlighted();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_justDisplayData) return;
        if (_isCancelOrBackButton) { Cancel(); return; }
        HandleSliderOn();
        SwitchUIDisplay();
    }

    public void OnSubmit(BaseEventData eventData) //KB/Ctrl
    {
        if (_justDisplayData) return;
        if (_isCancelOrBackButton) { Cancel(); return; }

        if (_amSlider)
        {
            if (_amSlider.interactable == true)
            {
                HandleSliderOff();
            }
            else
            {
                HandleSliderOn();
            }
        }
        SwitchUIDisplay();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_justDisplayData) return;
        _colours.SetUIColour(UIEventTypes.Selected);
        _invertColourCorrection.InvertColour(UIEventTypes.Selected);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_justDisplayData) return;
        _audio.Play(UIEventTypes.Pressed);
        _colours.SetUIColour(UIEventTypes.Highlighted);
        _invertColourCorrection.InvertColour(UIEventTypes.Highlighted);
    }

    public void OnMove(AxisEventData eventData)
    {
        if (_justDisplayData) return;
        if (_amSlider)
        {
            if (_amSlider.interactable == true)
            {
                if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
                {
                    _audio.Play(UIEventTypes.Pressed);
                }
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
        if (MyParentController)
        {
            Cancel();
        }
    }

    public void SetHighlight_Nothing_Selected()//color
    {
        _selected = false;
        _colours.SetUIColour(UIEventTypes.Normal);
        _colours.SetUIColour(UIEventTypes.Highlighted);
    }

    public void SetHighlighted()
    {
        _UICancelStopper.SetLastUIObject(gameObject);
        _colours.SetUIColour(UIEventTypes.Highlighted);
        _invertColourCorrection.InvertColour(UIEventTypes.Highlighted);
        _masterController.OnHoverOver(gameObject);
        _accessories.ActivatePointer(true);
        _buttonSize.HighlightedScaleUp();
    }

    private void SwitchUIDisplay()
    {
        _UICancelStopper.SetLastUIObject(gameObject);
        _masterController.SetLastSelected(this);
        _masterController.ClearOtherSelections(gameObject);
        _audio.Play(UIEventTypes.Pressed);
        _buttonSize.SelectedScaleUp();
        //Added a selected tracker to presever selected status on move off
        //Added a selected tracker to presever selected status on move off
        //Added a selected tracker to presever selected status on move off


        _swapImageOrText.Swap();
        StartCoroutine(_buttonSize.PressedSequence());

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
            if (!_amSlider)
            {
                _colours.OnSelectedColourChange(UIEventTypes.Selected);
                _invertColourCorrection.InvertColour(UIEventTypes.Selected);
            }
        }
    }

    private void DisableUIElement()
    {
        _buttonSize.SelectedScaleDown();
        _selected = false;
        _colours.OnSelectedColourChange(UIEventTypes.Highlighted);
        _UICancelStopper.RemoveFromTrackedList(_masterController);
    }

    private void ActivateUIElement()
    {
        _selected = true;
        _colours.OnSelectedColourChange(UIEventTypes.Selected);
        _invertColourCorrection.InvertColour(UIEventTypes.Selected);

        if (_childController)
        {
            _UICancelStopper.AddToTrackedList(_masterController);
            _UICancelStopper.AddToTrackedList(_childController);
            _masterController.MoveToChildLevel();
            _childController.FirstSelected(_masterController);
        }
    }

    public void NotHighlighted() //color
    {
        if (_selected)
        {
            _colours.SetUIColour(UIEventTypes.Selected);
            _invertColourCorrection.InvertColour(UIEventTypes.Selected);
        }
        else
        {
            _buttonSize.SelectedScaleDown();
            _colours.SetUIColour(UIEventTypes.Normal);
            _invertColourCorrection.InvertColour(UIEventTypes.Normal);
        }
        if(_amSlider) _amSlider.interactable = false;
        _buttonSize.HighlightedScaleDown();
        _swapImageOrText.Default();

        _accessories.ActivatePointer(false);
    }

    public void ClearUISelection()
    {
        _colours.SetUIColour(UIEventTypes.Normal);
        _invertColourCorrection.InvertColour(UIEventTypes.Normal);

        if (_childController && _selected)
        {
            _childController.MyCanvas.enabled = false;
        }
        _selected = false;
        _accessories.ActivatePointer(false);
        _buttonSize.SelectedScaleDown();
        _swapImageOrText.Default();
    }

    public void Cancel()
    {
        _audio.Play(UIEventTypes.Cancelled);
        _UICancelStopper.RemoveFromTrackedList(MyParentController);
        _buttonSize.SelectedScaleDown();
        _swapImageOrText.Default();

        if (MyParentController)
        {
            MyParentController.FirstSelected();
        }    
    }

    private void HandleSliderOn()
    {
        if (_amSlider)
        {
            _colours.SetUIColour(UIEventTypes.Selected);
            _invertColourCorrection.InvertColour(UIEventTypes.Selected);
            _amSlider.interactable = true;
        }
    }

    private void HandleSliderOff()
    {
        if (_amSlider)
        {
            _colours.SetUIColour(UIEventTypes.Highlighted);
            _invertColourCorrection.InvertColour(UIEventTypes.Highlighted);
            _amSlider.interactable = false;
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}


