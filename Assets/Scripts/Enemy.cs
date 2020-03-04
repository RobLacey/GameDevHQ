using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : EnemyController
{
    ISpawnable _myWave;

    protected override void Awake()
    {
        base.Awake();
        _myWave = GetComponentInParent<ISpawnable>();
    }

    public override void ProcessCollision() //IDamageable
    {
        if (_instanceHealth <= 0)
        {
            base.ProcessCollision();
            if (_myWave != null)
            {
                _myWave.LostEnemyFromWave();
            }            
        }
    }
}
