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
    [SerializeField] private AudioClip _disabled;
    [SerializeField] private float _disabledVolume;

    public AudioClip HighlightedClip => _highlighted;
    public AudioClip SelectedClip => _select;
    public AudioClip CancelledClip => _cancel;
    public AudioClip DisabledClip => _disabled;
    public float HighlightedVolume => _highlightedVolume;
    public float SelectedVolume => _selectedVolume;
    public float CancelledVolume => _cancelVolume;
    public float DisabledVolume => _disabledVolume;
}
