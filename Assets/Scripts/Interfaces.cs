using UnityEngine;

public interface IDamageable
    {
    void Damage(float damage = 0);
    }

public interface ISpawnable
{
    Vector3 SpawnPosition();
}
