using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownTimer : MonoBehaviour
{
    [SerializeField] EventManager _Event_CountDownTimer; 
    [SerializeField] EventManager _Event_PlayerDead;

    Text _countDownText = default;

    private void Awake()
    {
        _countDownText = GetComponent<Text>();
    }
    private void Start()
    {
        _countDownText.enabled = false;
    }
    private void OnEnable()
    {
        _Event_CountDownTimer.AddListener((x) => Countdown(x));
        _Event_PlayerDead.AddListener(() => OnPlayerDeath());
    }

    private void Countdown(object value)
    {
        float newValue = (float)value;
        if (newValue > 0)
        {
            _countDownText.enabled = true;
            _countDownText.text = newValue.ToString("0.0");
        }
        else
        {
            _countDownText.enabled = false;
        }
    }

    private void OnPlayerDeath()
    {
        _countDownText.enabled = false;
    }
}
