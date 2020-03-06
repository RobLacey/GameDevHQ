using UnityEngine;
using System;

public interface IDamageable : ITagable
{
    void ProcessCollision(int damage);
}

public interface ITagable
{
    int TeamTag { get; set; }
}

public interface IKillable
{
    void Dead();
}


public interface ISpeedBoostable
{
    float SetSpeed { set; }
}

public interface IWeaponSystem 
{
    void Fire();
    bool ShieldsAreActive { get; set; }
    float ReturnFireRate();
    void DeactivatePowerUps(PowerUpTypes oldPowerUp);
}

public interface ISpawnable
{
    void ActivateChildObjects(bool activate);
    void LostEnemyFromWave();
}

public interface IScaleable
{
    void SetScale(Vector3 scale);
}




