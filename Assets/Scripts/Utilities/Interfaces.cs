using UnityEngine;
using System;

public interface IDamageable : ITagable
{
    void I_ProcessCollision(int damage);
}

public interface ITagable
{
    TeamID I_TeamTag { get; }

}

public interface IKillable
{
    void I_Dead(bool collsionKill);
}


public interface ISpeedBoostable
{
    float I_SetSpeed { set; }
}

public interface IWeaponSystem 
{
    void I_Fire();
    bool I_ShieldsAreActive { get; set; }
    float I_ReturnFireRate();
    void I_DeactivatePowerUps(PowerUpTypes oldPowerUp);
}

public interface IEnemyWave
{
    void I_DeactivateChildObjects();
    void I_ActivateChildObjects(bool activate);
    void I_LostEnemyFromWave(int score);
}

public interface IScaleable
{
    void I_SetScale(Vector3 scale);
}




