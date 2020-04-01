using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] TeamID _teamID;
    [SerializeField] protected AudioClip _collectSFX = default;

    //Variables
    SpriteRenderer[] _myBody;
    protected AudioSource _audioSource;
    protected Collider2D _collider2D;

    //Properties
    public TeamID TeamTag { get { return _teamID; } }

    protected void Awake()
    {
        _collider2D = GetComponentInChildren<Collider2D>();
        _myBody = GetComponentsInChildren<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _collectSFX;
    }

    private void OnEnable()
    {
        ActivateSprite(true);
        _collider2D.enabled = true;
        transform.position = new Vector3(transform.position.x, transform.position.y, -1); //Adjusts for gfx error with emission
    }

    protected void ActivateSprite(bool active)
    {
        foreach (var sprite in _myBody)
        {
            sprite.enabled = active;
        }
    }

    protected IEnumerator DisableObject(float timer)
    {
        yield return new WaitForSeconds(timer);
        gameObject.SetActive(false);
    }
}
