﻿using UnityEngine;
using System;

public interface IDamageable : ITagable
{
    void I_ProcessCollision(float damage);
}

public interface ITagable
{
    TeamID I_TeamTag { get; }

}

public interface IKillable
{
    void I_Dead(bool collsionKill);
}


public interface IEnemyWave
{
    void I_DeactivateChildObjects();
    void I_LostEnemyFromWave(int score, Vector3 lastEnemiesPosition);
}

public interface IShowDamage
{
    void DamageDisplay(float damageCalc);
}

public interface IScaleable
{
    void I_SetScale(Vector3 scale);
}

public interface IWaveMemeber
{
    GameObject Myself { get; }
}





