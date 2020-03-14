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
    float I_ReturnFireRate();
    void I_ActivateWeapon(object newWeapon);
    void I_DeactivateWeapon(object oldWeapon);
}

public interface IPowerUpSystem
{
    bool I_ShieldsAreActive { get; set; } 

    void I_ActivatePowerUp(object newPowerUp);

    void I_DeactivatePowerUps(object oldPowerUp);

}

public interface IEnemyWave
{
    void I_DeactivateChildObjects();
    void I_LostEnemyFromWave(int score, Vector3 lastEnemiesPosition);
}

public interface IShrapnel
{
    void I_DeactivateChildObjects();
    //void I_LostShrapnelPiece();
}

public interface IScaleable
{
    void I_SetScale(Vector3 scale);
}

public interface IWaveMemeber
{
    GameObject Myself { get; }
}




