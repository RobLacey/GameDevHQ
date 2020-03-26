using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActivator : MonoBehaviour
{
    [SerializeField] PowerUpTypes powerUpTypes;
    [SerializeField] EventManager _Event_ActivatePowerUp = default;
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_UpdateWeaponUI;
    [SerializeField] bool _isWeapon = false;

    Canvas _UI;
    private void Awake()
    {
        _UI = GetComponent<Canvas>();
        _UI.enabled = false;
    }

    private void OnEnable()
    {
        _Event_ActivatePowerUp.AddListener(x => ActivatePowerUPUI(x));
        _Event_DeactivatePowerUp.AddListener(x => DeactviatePowerUPUI(x));
        _Event_UpdateWeaponUI.AddListener(x => ActivateWeaponUI(x));
    }

    private void ActivatePowerUPUI(object type)
    {
        if ((PowerUpTypes)type == powerUpTypes && !_isWeapon)
        {
            _UI.enabled = true;
        }
    }

    private void ActivateWeaponUI(object type)
    {
        if ((PowerUpTypes)type == powerUpTypes && _isWeapon)
        {
            _UI.enabled = true;
        }
        else
        {
            _UI.enabled = false;
        }
    }

    private void DeactviatePowerUPUI(object type)
    {
        if ((PowerUpTypes)type == powerUpTypes && !_isWeapon)
        {
            _UI.enabled = false;
        }
    }

}
