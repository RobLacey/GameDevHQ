using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VisualDamage : MonoBehaviour, IShowDamage
{
    [SerializeField] Damage[] _damagesSteps = default;

    [Serializable]
    public class Damage
    {
        [SerializeField] public float _damagePerc;
        [SerializeField] public GameObject _damageFX;
    }

    private void OnEnable()
    {
        foreach (var item in _damagesSteps)
        {
            item._damageFX.SetActive(false);
        }
    }

    public void DamageDisplay(float damageCalc)
    {
        foreach (var item in _damagesSteps)
        {
            if (item._damagePerc > damageCalc)
            {
                item._damageFX.SetActive(true);
            }
            else
            {
                item._damageFX.SetActive(false);
            }
        }
    }
}
