using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class PunchData
{
    [SerializeField]
    [InfoBox("DOESN'T use Global Tween Time.")]
    [AllowNesting] public EffectType _punchWhen = EffectType.In;
    [SerializeField]  Vector3 _strength = new Vector3(0.1f, 0.1f, 0f);
    [SerializeField] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [Range(0, 1)] float _elasticity = 0.5f;
    [SerializeField] [Range(1, 10)] int _vibrato = 5;

    public EffectType PunchWhen => _punchWhen;

    public Vector3 Strength => _strength;

    public float Duration => _duration;

    public float Elasticity => _elasticity;

    public int Vibrato => _vibrato;
}