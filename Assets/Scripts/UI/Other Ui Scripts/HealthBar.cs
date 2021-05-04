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
    [SerializeField] EventManager _Event_DeactivatePowerUp;
    [SerializeField] EventManager _Event_ActivatePowerUp = default;


    AudioSource _audioSource;
    Coroutine coroutine = null;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _Event_SetLives.AddListener(y => SetLivesDisplay(y), this);
        _Event_PlayerDead.AddListener(() => _audioSource.Stop(), this);
        _Event_ActivatePowerUp.AddListener(x => AddHealth(x), this);
    }

    private void SetLivesDisplay(object lives)
    {
        float currentHealth = Mathf.Clamp((float)lives, 0, 1);
        _healthUI.fillAmount = currentHealth;
        _healthText.text = Mathf.Clamp(((float)lives * 10), 0, 10).ToString("F0");
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

    private void AddHealth(object isHealth)
    {
        if ((PowerUpTypes)isHealth == PowerUpTypes.Health)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(HealthDisplay());
        }
    }

    IEnumerator HealthDisplay()
    {
        yield return new WaitForSeconds(3f);
        _Event_DeactivatePowerUp.Invoke(PowerUpTypes.Health, this);
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
