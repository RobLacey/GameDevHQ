using UnityEngine;

[CreateAssetMenu(fileName = "Sound Scheme", menuName = "UIElements Schemes / New Audio Scheme")]
public class AudioScheme : ScriptableObject
{
    [SerializeField] private AudioClip _highlighted = default;
    [SerializeField] private float _highlightedVolume = default;
    [SerializeField] private AudioClip _select = default;
    [SerializeField] private float _selectedVolume = default;
    [SerializeField] private AudioClip _cancel = default;
    [SerializeField] private float _cancelVolume = default;
    [SerializeField] private AudioClip _disabled = default;
    [SerializeField] private float _disabledVolume = default;

    public AudioClip HighlightedClip => _highlighted;
    public AudioClip SelectedClip => _select;
    public AudioClip CancelledClip => _cancel;
    public AudioClip DisabledClip => _disabled;
    public float HighlightedVolume => _highlightedVolume;
    public float SelectedVolume => _selectedVolume;
    public float CancelledVolume => _cancelVolume;
    public float DisabledVolume => _disabledVolume;
}
