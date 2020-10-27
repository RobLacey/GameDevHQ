using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound Scheme", menuName = "UIElements Schemes / New Audio Scheme")]
public class AudioScheme : ScriptableObject
{
    [SerializeField] private AudioClip _highlighted;
    [SerializeField] private float _highlightedVolume;
    [SerializeField] private AudioClip _select;
    [SerializeField] private float _selectedVolume;
    [SerializeField] private AudioClip _cancel;
    [SerializeField] private float _cancelVolume;

    public AudioClip HighlightedClip => _highlighted;
    public AudioClip SelectedClip => _select;
    public AudioClip CancelledClip => _cancel;
    public float HighlighVolume => _highlightedVolume;
    public float SelectedVolume => _selectedVolume;
    public float CancelledVolume => _cancelVolume;
}
