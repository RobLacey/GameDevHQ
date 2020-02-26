using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : EnemyController
{
    [SerializeField] AudioClip _explosionSFX = default;

    //Variables
    EnemyFireControl _fireControl;

    protected override void Awake()
    {
        base.Awake();
        _audioSource.clip = _explosionSFX;
        _fireControl = GetComponent<EnemyFireControl>();
    }

    public override void ProcessCollision() //IDamageable
    {
        if (_health <= 0)
        {
            base.ProcessCollision();
            _fireControl.StopAllCoroutines();
            SetSpeed(0);
        }
    }
}
