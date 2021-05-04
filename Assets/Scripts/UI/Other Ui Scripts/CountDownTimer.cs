using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownTimer : MonoBehaviour
{
    [SerializeField] EventManager _Event_CountDownTimer; 
    [SerializeField] EventManager _Event_PlayerDead;

    Text _countDownText = default;
    Canvas _countDownCanvas = default;

    private void Awake()
    {
        _countDownText = GetComponent<Text>();
        _countDownCanvas = GetComponent<Canvas>();
    }
    private void Start()
    {
        _countDownCanvas.enabled = false;
    }
    private void OnEnable()
    {
        _Event_CountDownTimer.AddListener((x) => Countdown(x), this);
        _Event_PlayerDead.AddListener(() => OnPlayerDeath(), this);
    }

    private void Countdown(object value)
    {
        float newValue = (float)value;
        if (newValue > 0)
        {
            _countDownCanvas.enabled = true;
            _countDownText.text = newValue.ToString("0.0");
        }
        else
        {
            _countDownCanvas.enabled = false;
        }
    }

    private void OnPlayerDeath()
    {
        _countDownText.enabled = false;
    }
}
