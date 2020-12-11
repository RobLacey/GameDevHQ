using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class PunchData
{
    [SerializeField] 
    [InfoBox("DOESN'T use Global Tween Time.")] 
    private IsActive _atEndOfTweens = IsActive.No;
    [SerializeField] private Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] [Range(0, 2)] private float _duration = 0.5f;
    [SerializeField] [Range(0, 1)] private float _elasticity = 0.5f;
    [SerializeField] [Range(1, 10)] private int _vibrato = 5;

    public Vector3 Strength => _strength;

    public float Duration => _duration;

    public float Elasticity => _elasticity;

    public int Vibrato => _vibrato;
    public bool EndTween => _atEndOfTweens == IsActive.Yes;
}