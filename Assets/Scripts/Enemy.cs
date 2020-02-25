using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : EnemyController
{
    [SerializeField] AudioClip _explosionSFX = default;

    protected override void Awake()
    {
        base.Awake();
        _audioSource.clip = _explosionSFX;
    }

    public override void ProcessCollision() //IDamageable
    {
        if (_health <= 0)
        {
            base.ProcessCollision();
            SetSpeed(0);
            //Instantiate(_expolsion, transform.position, Quaternion.identity, transform);
        }
    }
}
