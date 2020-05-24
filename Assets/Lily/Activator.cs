using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
    [SerializeField] GameObject _activateThis;
    AudioSource _audioSource;

    private void Start()
    {
        if(_activateThis) _activateThis.SetActive(false);
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_activateThis) _activateThis.SetActive(true);
        _audioSource.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if (_activateThis) _activateThis.SetActive(false);
        _audioSource.Stop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != gameObject.tag)
        {
            _audioSource.Play();
        }
    }
}
