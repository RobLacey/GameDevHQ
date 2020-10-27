using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "Colour Scheme", menuName = "UIElements Schemes / New Colour Scheme")]
public class ColourScheme : ScriptableObject
{
    [SerializeField] private Color _disabled = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color _selected = Color.white;
    [SerializeField] private Color _highlighted = Color.white;
    [SerializeField] private Color _pressedFlash = Color.white;
    [SerializeField] [EnumFlags] private EventType _coloursToUse;
    [SerializeField] [Range(0, 2)] private float _tweenTime = 0.4f;
    [SerializeField] [Range(0.5f, 2f)] private float _selectedHighlightPerc = 1f;
    [SerializeField] [Range(0, 0.5f)] private float _pressedFlashTime = 0.1f;


    public Color DisableColour => _disabled;
    public Color SelectedColour => _selected;
    public Color HighlightedColour => _highlighted;
    public Color PressedColour => _pressedFlash;
    public EventType ColourSettings => _coloursToUse;
    public float TweenTime => _tweenTime;
    public float SelectedPerc => _selectedHighlightPerc;
    public float FlashTime => _pressedFlashTime;
}
