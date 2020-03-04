using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] AudioClip _sfx;
    [SerializeField] float _flashLength = 0.3f;
    [SerializeField] float _volume = 0.2f;

    AudioSource _audioSource;
    SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _sfx;
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _spriteRenderer.enabled = true;
        _audioSource.volume = _volume;
        _audioSource.Play();
        StartCoroutine(Timer(_sfx.length, _flashLength));
    }

    private IEnumerator Timer(float activateTimer, float flashLength)
    {
        yield return new WaitForSeconds(flashLength);
        _spriteRenderer.enabled = false;
        yield return new WaitForSeconds(activateTimer - flashLength);
        gameObject.SetActive(false);
    }
}
