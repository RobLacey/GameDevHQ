using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expolsion : MonoBehaviour, IScaleable
{
    [SerializeField] AudioClip _explosionSFX = default;

    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _explosionSFX;
    }

    private void OnEnable()
    {
        _audioSource.Play();
        transform.Random2DRotation();
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    public void Destroy()
    {
        gameObject.SetActive(false);
    }

}
