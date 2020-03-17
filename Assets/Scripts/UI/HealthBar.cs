using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image _healthUI;
    [SerializeField] Text _healthText;
    [SerializeField] Color[] _healthBarColour;
    [SerializeField] float _flashSpeed = 1f;
    [SerializeField] EventManager _Event_SetLives = default;
    [SerializeField] EventManager _Event_PlayerDead = default;

    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _Event_SetLives.AddListener(y => SetLivesDisplay(y));
        _Event_PlayerDead.AddListener(() => _audioSource.Stop());
    }

    public void SetLivesDisplay(object lives)
    {
        float currentHealth = Mathf.Clamp((float)lives, 0, 1);
        _healthUI.fillAmount = currentHealth;
        _healthText.text = ((float)lives * 10).ToString("F0");
        SetHealthBarColour(currentHealth);
    }

    private void SetHealthBarColour(float currentHealth)
    {
        if (currentHealth < 0.66f)
        {
            if (currentHealth < 0.33f)
            {
                StartCoroutine(FlashDisplay());
                _audioSource.Play();
                _healthUI.color = _healthBarColour[2];
            }
            else
            {
                ResetWarnings();
                _healthUI.color = _healthBarColour[1];
            }
        }
        else
        {
            ResetWarnings();
            _healthUI.color = _healthBarColour[0];
        }
    }

    private void ResetWarnings()
    {
        _healthUI.enabled = true;
        StopAllCoroutines();
        _audioSource.Stop();
    }

    IEnumerator FlashDisplay()
    {
        while (true)
        {
            _healthUI.enabled = true;
            yield return new WaitForSeconds(_flashSpeed);
            _healthUI.enabled = false;
            yield return new WaitForSeconds(_flashSpeed);
        }    
    }
}
