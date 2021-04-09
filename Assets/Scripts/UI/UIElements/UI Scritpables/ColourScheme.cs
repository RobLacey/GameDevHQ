using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "Colour Scheme", menuName = "UIElements Schemes / New Colour Scheme")]
public class ColourScheme : ScriptableObject
{
    [Header("Colours", order = 2)] 
    [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 3)]
    [SerializeField] private Color _disabled = Color.grey;
    [SerializeField] private Color _selected = Color.blue;
    [SerializeField] private Color _highlighted = Color.yellow;
    [SerializeField] private Color _pressedFlash = Color.black;
    
    [Header("Colour Tween Values", order = 2)] 
    [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 3)]
    [SerializeField] [EnumFlags] private EventType _coloursToUse = EventType.None;
    [SerializeField] [Range(0, 2)] private float _tweenTime = PresetTween;
    [SerializeField] [Range(0.5f, 2f)] private float _selectedHighlightPerc = PresetSelectedHighlight;
    [SerializeField] [Range(0, 0.5f)] private float _pressedFlashTime = PresetFlashTime;
    
    [Button("Reset Values To Default")]
    private void ResetValues()
    {
        _tweenTime = PresetTween;
        _selectedHighlightPerc = PresetSelectedHighlight;
        _pressedFlashTime = PresetFlashTime;
    }
    
    [Button("Reset Colours To Default")]
    private void ResetColours()
    {
        _disabled = Color.gray;
        _selected = Color.blue;
        _highlighted = Color.yellow;
        _pressedFlash = Color.black;
    }

    private const float PresetTween = 0.4f;
    private const float PresetSelectedHighlight = 1f;
    private const float PresetFlashTime = 0.1f;

    public Color DisableColour => _disabled;
    public Color SelectedColour => _selected;
    public Color HighlightedColour => _highlighted;
    public Color PressedColour => _pressedFlash;
    public EventType ColourSettings => _coloursToUse;
    public float TweenTime => _tweenTime;
    public float SelectedPerc => _selectedHighlightPerc;
    public float FlashTime => _pressedFlashTime;
}
