using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : EnemyController
{
    [SerializeField] AudioClip _explosion;
    //[SerializeField] GameObject _expolsion = default;

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
