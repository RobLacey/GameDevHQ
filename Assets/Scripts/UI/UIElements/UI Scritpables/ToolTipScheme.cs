
using System;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "UIElements Schemes / New ToolTip Scheme ", fileName = "ToolTipScheme")]
public class ToolTipScheme : ScriptableObject
{
    [InfoBox("FOLLOW - Follows Mouse or Virtual Cursor or RectTransform of Node for Keys or Controller \n \n" + 
             "FIXED POSITION - Everything. Tooltip appears in the position of the supplied RectTransform")]
    
    [Header("Mouse & Virtual Cursor Settings", order = 2)] 
    [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 3)]
    [SerializeField]
    [Label("Tooltip Type")]
    private TooltipType _tooltipTypeMouse = TooltipType.Follow;
    
    [SerializeField] 
    [AllowNesting] [Label("Tooltip Position")] 
    private ToolTipAnchor _toolTipPosition = default;
    
    [SerializeField] 
    [Range(-100f, 100f)] [Label("X Padding (-100 to 100)")]
    private float _mousePaddingX = default;
    
    [SerializeField] 
    [Range(-100F, 100F)] [Label("Y Padding (-100 to 100)")]
    private float _mousePaddingY = default;
    
    [Header("Keyboard and Controller Settings")] 
    [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    
    [SerializeField] 
    [Label("Tooltip Type")]
    private TooltipType _tooltipTypeKeys = TooltipType.Follow;

    [SerializeField] 
    [Label("Tooltip Preset")]
    private UseSide _positionToUse = UseSide.ToTheRightOf;
    
    [SerializeField] 
    [Label("Offset Position")]
    private ToolTipAnchor _keyboardPosition = default;
    
    [SerializeField]  
    [Range(-100F, 100F)] [Label("X Padding (-100 to 100)")] 
    private float _keyboardPaddingX = default;
    
    [SerializeField] 
    [Range(-100f, 100)] [Label("Y Padding (-100 to 100)")]
    private float _keyboardPaddingY = default;
    
    [Header("Other Settings")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    
    [SerializeField] 
    [Range(0f, 50f)] [Label("Screen Edge Safe Zone")]
    private float _screenSafeZone = 10;
    
    [SerializeField] 
    [Range(0.1f, 5f)] 
    private float _displayTooltipDelay = 1f;
    
    [SerializeField] 
    [Range(0.1f, 5f)] 
    private float _buildDelay = 1f;
    
    [Header("FadeTween Settings")] [Space(10f)] [HorizontalLine(1, color: EColor.Blue, order = 1)]
    
    [SerializeField] 
    [Range(0.1f, 1f)]
    private float _fadeInTweenTime = 0.2f;
    
    [SerializeField] 
    [Range(0.1f, 1f)]
    private float _fadeOutTweenTime = 0.2f;


    public TooltipType ToolTipTypeMouse => _tooltipTypeMouse;
    public TooltipType ToolTipTypeKeys => _tooltipTypeKeys;
    public ToolTipAnchor ToolTipPosition => _toolTipPosition;
    public float MouseXPadding => _mousePaddingX;
    public float MouseYPadding => _mousePaddingY;
    public UseSide PositionToUse => _positionToUse;
    public ToolTipAnchor KeyboardPosition => _keyboardPosition;
    public float KeyboardXPadding => _keyboardPaddingX;
    public float KeyboardYPadding => _keyboardPaddingY;
    public float ScreenSafeZone => _screenSafeZone;
    public float StartDelay => _displayTooltipDelay;
    public float BuildDelay => _buildDelay;
    public float FadeInTime => _fadeInTweenTime;
    public float FadeOutTime => _fadeOutTweenTime;

    public bool Fixed() => _tooltipTypeMouse == TooltipType.FixedPosition;
    public bool Follow() => _tooltipTypeMouse == TooltipType.Follow;

}
