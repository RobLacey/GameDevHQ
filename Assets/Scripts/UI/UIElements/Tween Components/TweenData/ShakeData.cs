using System;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public class ShakeData
{
    [SerializeField]
    [InfoBox("DOESN'T use Global Tween Time.")]
    [AllowNesting] public EffectType _shakeWhen = EffectType.In;
    [SerializeField] Vector3 _strength = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] [Range(0, 2)] float _duration = 0.5f;
    [SerializeField] [Range(1, 10)] int _vibrato = 10;
    [SerializeField] [Range(0, 90)] float _randomness = 45f;
    [SerializeField] bool _fadeOut = true;

    public EffectType ShakeWhen => _shakeWhen;

    public Vector3 Strength => _strength;

    public float Duration => _duration;

    public int Vibrato => _vibrato;

    public float Randomness => _randomness;

    public bool FadeOut => _fadeOut;
}