using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expolsion : MonoBehaviour, IScaleable
{
    [SerializeField] AudioClip _explosionSFX = default;
    [SerializeField] float _minPicth = 0.8f;
    [SerializeField] float _maxPitch = 1.2f;
    [SerializeField] float _minVolume = 0.8f;
    [SerializeField] float _maxVolume = 1f;

    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _explosionSFX;
    }

    private void OnEnable()
    {
        _audioSource.volume = Random.Range(_minVolume, _maxVolume);
        _audioSource.pitch = Random.Range(_minPicth, _maxPitch);
        _audioSource.Play();
        transform.Random2DRotation();
    }

    public void I_SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    public void Destroy()
    {
        gameObject.SetActive(false);
    }

}
