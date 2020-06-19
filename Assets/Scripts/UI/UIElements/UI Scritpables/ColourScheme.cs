using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "Colour Scheme", menuName = "New Colour Scheme")]
public class ColourScheme : ScriptableObject
{
    [SerializeField] Color _disabled = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] Color _selected = Color.white;
    [SerializeField] Color _highlighted = Color.white;
    [SerializeField] Color _pressedFlash = Color.white;
    [SerializeField] [EnumFlags] EventType _coloursToUse;
    [SerializeField] [Range(0, 2)] float _tweenTime = 0.4f;
    [SerializeField] [Range(0.5f, 2f)] float _selectedHighlightPerc = 1f;
    [SerializeField] [Range(0, 0.5f)] float _pressedFlashTime = 0.1f;


    public Color DisableColour { get { return _disabled; } }
    public Color SelectedColour { get { return _selected; } }
    public Color HighlightedColour { get { return _highlighted; } }
    public Color PressedColour { get { return _pressedFlash; } }
    public EventType ColourSettings { get { return _coloursToUse; } }
    public float TweenTime { get { return _tweenTime; } }
    public float SelectedPerc { get { return _selectedHighlightPerc; } }
    public float FlashTime { get { return _pressedFlashTime; } }
}
