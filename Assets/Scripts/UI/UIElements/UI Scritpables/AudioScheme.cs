using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound Scheme", menuName = "New Audio Scheme")]
public class AudioScheme : ScriptableObject
{
    [SerializeField] AudioClip _highlighted;
    [SerializeField] float _highlightedVolume;
    [SerializeField] AudioClip _select;
    [SerializeField] float _selectedVolume;
    [SerializeField] AudioClip _cancel;
    [SerializeField] float _cancelVolume;

    public AudioClip HighlightedClip { get { return _highlighted; } }
    public AudioClip SelectedClip { get { return _select; } }
    public AudioClip CancelledClip { get { return _cancel; } }
    public float HighlighVolume { get { return _highlightedVolume; } }
    public float SelectedVolume { get { return _selectedVolume; } }
    public float CancelledVolume { get { return _cancelVolume; } }
}
