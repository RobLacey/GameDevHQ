using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour
{
    [SerializeField] Toggle _button;
    [SerializeField] GameObject _UIElement;
    [SerializeField] Color _selectedColour;
    [SerializeField] Color _normalColour;
    [SerializeField] bool _cancelOnPress;
    [SerializeField] string _cancelOnPressAxis;

    //Variables
    EventSystem _eventSystem;

    private void Awake()
    {
        _eventSystem = FindObjectOfType<EventSystem>();
        _normalColour = _button.colors.normalColor;
    }

    private void Update()
    {
        if (Input.GetAxis(_cancelOnPressAxis) != 0 &&  _cancelOnPress)
        {
            Cancel();
        }
    }

    public void Cancel()
    {
        _button.colors = _button.colors.SwapUIColour(_normalColour);
        _button.isOn = false;
    }

    public void Open()
    {
        if (_button.isOn)
        {
            _button.colors = _button.colors.SwapUIColour(_selectedColour);
            if (_UIElement != null)
            {
                _eventSystem.SetSelectedGameObject(_UIElement);

            }        
        }
        else
        {
            _button.colors = _button.colors.SwapUIColour(_normalColour);
        }
    }
}
