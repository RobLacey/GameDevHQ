using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ColourLerp))]
public class UIElementSetUp : MonoBehaviour, IEndDragHandler, IPointerEnterHandler, IPointerDownHandler,
                                                ISelectHandler, ISubmitHandler, IPointerExitHandler,
                                               IMoveHandler, IBeginDragHandler
{
    [SerializeField] UIEventTypes _settings = UIEventTypes.Normal;
    [SerializeField] LevelNode _childController;
    [SerializeField] bool _isCancelOrBackButton;
    [SerializeField] PreserveSelection _preseveSelection;
    [SerializeField] UIColour _colours;
    [SerializeField] public UIAudio _audio;
    [SerializeField] UIAccessories _accessories;
    [SerializeField] UISize _buttonSize;
    [SerializeField] UIInvertColours _invertColourCorrection;
    [SerializeField] UISwapper _swapImageOrText;

    //Variables
    bool _selected = false;
    LevelNode _masterController;
    UIManager _UIManager;
    Slider _amSlider;

    enum PreserveSelection { Never, UnitlNewSelection, UntilCancelled_ToggleOnly }

    public LevelNode MyParentController { get; set; }

    public UIEventTypes ButtonStatus
    {
        get { return _settings; }
        set
        {
            _settings = value;
            SetButton();
        }
    }

    private void Awake()
    {
        _UIManager = FindObjectOfType<UIManager>();
        if(TryGetComponent(out _amSlider))
        {
            _amSlider.interactable = false;
            if (_preseveSelection != PreserveSelection.Never)
            {
                _preseveSelection = PreserveSelection.Never;
                Debug.Log("Can't use Slider in toggle group or as always selected");
            }
        }
        _colours.MyColourLerper = GetComponent<ColourLerp>();
        _masterController = GetComponentInParent<LevelNode>();
        _colours.OnAwake();
        _audio.OnAwake();
        _accessories.OnAwake();
        _buttonSize.OnAwake(transform);
        _invertColourCorrection.OnAwake();
        _swapImageOrText.OnAwake();
        _audio.MyAudiosource = GetComponentInParent<AudioSource>();

        ButtonStatus = UIEventTypes.Normal;
    }

    public void OnPointerEnter(PointerEventData eventData) //Mouse highlight
    {
        if (eventData.pointerDrag) return; //Enables drag on slider to have pressed colour
        _UIManager.OnHoverOver(gameObject);
        _audio.Play(UIEventTypes.Highlighted);

        ButtonStatus = UIEventTypes.Highlighted;
    }

    public void OnSelect(BaseEventData eventData) //KB/Ctrl highlight
    {
        _masterController.ResetLevel();
        ButtonStatus = UIEventTypes.Highlighted;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag) return; //Enables drag on slider to have pressed colour
        HandleSliderOff();
        SetNotHighlighted();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isCancelOrBackButton) { ButtonStatus  = UIEventTypes.Cancelled; return; }
        HandleSliderOn();
        SwitchUIDisplay();
    }

    public void OnSubmit(BaseEventData eventData) //KB/Ctrl
    {
        if (_isCancelOrBackButton) { ButtonStatus = UIEventTypes.Cancelled; return; }

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
        _colours.SetUIColour(UIEventTypes.Selected);
        _invertColourCorrection.InvertColour(UIEventTypes.Selected);
        ButtonStatus = UIEventTypes.Selected;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _audio.Play(UIEventTypes.Pressed);
        HandleSliderOff();
        _colours.SetUIColour(UIEventTypes.Highlighted);
        _invertColourCorrection.InvertColour(UIEventTypes.Highlighted);
        ButtonStatus = UIEventTypes.Highlighted;
    }

    public void OnMove(AxisEventData eventData)
    {
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

    public void SetButton()
    {
        switch (_settings)
        {
            case UIEventTypes.Normal:
                SetNormal();
                break;
            case UIEventTypes.Highlighted:
                SetHighlighted();
                break;
            case UIEventTypes.Selected:
                SetSelected();
                break;
            case UIEventTypes.Pressed:
                SetPressed();
                break;
            case UIEventTypes.Cancelled:
                NewOnCancel();
                break;
            default:
                break;
        }
    }

    private void SwitchUIDisplay()
    {
        _masterController.SaveLastSelected(this);
        _masterController.ResetLevel();

        if (_selected)
        {
            _audio.Play(UIEventTypes.Cancelled);
            DisableChildLevel();
        }
        else
        {
            _audio.Play(UIEventTypes.Pressed);
            ActivateChildLevel();
        }

        ButtonStatus = UIEventTypes.Pressed;
    }

    public void DisableChildLevel()
    {
        _selected = false;

        if (_childController)
        {
            _childController.MyCanvas.enabled = false;
        }
    }

    private void NewOnCancel()
    {
        if (!MyParentController)
        {
            return;
        }
        _audio.Play(UIEventTypes.Cancelled);
        _masterController.MyCanvas.enabled = false;
        _selected = false;

        if (MyParentController)
        {
            MyParentController.MoveToNextLevel();

            if (_preseveSelection != PreserveSelection.UntilCancelled_ToggleOnly)
            {
                _buttonSize.SelectedScaleDown();
                _swapImageOrText.Default();
            }
        }
        ButtonStatus = UIEventTypes.Normal;
    }

    private void ActivateChildLevel()
    {
        _selected = true;

        if (_childController)
        {
            if (_preseveSelection == PreserveSelection.UntilCancelled_ToggleOnly)
            {
                _childController.MyCanvas.enabled = true;
            }
            else
            {
                _masterController.OnMovingToChildLevel();
                _childController.MoveToNextLevel(_masterController);
            }
        }
    }

    public void SetNotHighlighted()
    {
        if (_preseveSelection == PreserveSelection.Never)
        {
            DisableChildLevel();
        }

        if (!_selected)
        {
            ButtonStatus = UIEventTypes.Normal;
        }
        else
        {
            ButtonStatus = UIEventTypes.Selected;
        }
    }

    private void SetHighlighted()
    {
        _colours.SetUIColour(UIEventTypes.Highlighted);
        _invertColourCorrection.InvertColour(UIEventTypes.Highlighted);
        _accessories.ActivatePointer(true);
        _buttonSize.HighlightedScaleUp();
    }

    private void SetNormal()
    {

        _buttonSize.SelectedScaleDown();
        _colours.SetUIColour(UIEventTypes.Normal);
        _invertColourCorrection.InvertColour(UIEventTypes.Normal);
        _swapImageOrText.Default();
        _accessories.ActivatePointer(false);

        if (_amSlider) _amSlider.interactable = _selected;
        _buttonSize.HighlightedScaleDown();

    }

    private void SetSelected()
    {
        _colours.SetUIColour(UIEventTypes.Selected);
        _invertColourCorrection.InvertColour(UIEventTypes.Selected);
        _buttonSize.SelectedScaleUp();
    }

    private void SetPressed()
    {
        if (_selected)
        {
            _buttonSize.SelectedScaleUp();
            _swapImageOrText.Swap();
            StartCoroutine(_buttonSize.PressedSequence());
            _colours.OnSelectedColourChange(UIEventTypes.Selected);
            _invertColourCorrection.InvertColour(UIEventTypes.Selected);

        }
        else
        {
            _buttonSize.SelectedScaleDown();
            _swapImageOrText.Default();
            _invertColourCorrection.InvertColour(UIEventTypes.Highlighted);
            StartCoroutine(_buttonSize.PressedSequence());
            _colours.OnSelectedColourChange(UIEventTypes.Highlighted);
        }
    }

    public void ResetUI()
    {
        if (_preseveSelection != PreserveSelection.UntilCancelled_ToggleOnly)
        {
            if (ButtonStatus != UIEventTypes.Normal) { ButtonStatus = UIEventTypes.Normal; }
            DisableChildLevel();
        }
        else
        {
            SetNotHighlighted();
        }
    }


    private void HandleSliderOn()
    {
        if (_amSlider)
        {
            _colours.SetUIColour(UIEventTypes.Selected);
            _invertColourCorrection.InvertColour(UIEventTypes.Selected);
            _amSlider.interactable = true;
            EventSystem.current.SetSelectedGameObject(gameObject);
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


