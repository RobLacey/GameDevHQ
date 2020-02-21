using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [SerializeField] int _points = default;
    [SerializeField] string _playerTag = default;
    [SerializeField] string _animationTrigger = default;
    [SerializeField] SpriteRenderer _myShip = default;
    [SerializeField] AudioClip _explosion;

    //Variables
    UIManger _uiManger;
    Animator _myAnimator;
    NonPlayerMovement myMovement;

    private void Start()
    {
        _uiManger = FindObjectOfType<UIManger>();
        _myAnimator = GetComponentInChildren<Animator>();
        myMovement = GetComponent<NonPlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable canDealDamage = collision.GetComponent<IDamageable>();

        if (canDealDamage != null)
        {
            if (collision.tag != _playerTag)
            {
                _uiManger.AddToScore(_points);
            }
            canDealDamage.Damage();
            DeathProcess();
        }
    }

    private void DeathProcess()
    {
        GetComponent<Collider2D>().enabled = false;
        _myShip.enabled = false;
        myMovement.DontRespawn();
        _myAnimator.SetTrigger(_animationTrigger);
    }

    public void OnDestroy()
    {
        Destroy(gameObject);
    }
}
