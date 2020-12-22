
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "UIElements Schemes / New ToolTip Scheme ", fileName = "ToolTipScheme")]
public class ToolTipScheme : ScriptableObject
{
    [SerializeField] private TooltipType _tooltipType = TooltipType.Follow;
    
    [Header("Mouse & Global Fixed Position Settings", order = 2)]
    [SerializeField] 
    [AllowNesting] [Label("Tooltip Position")] private ToolTipAnchor _toolTipPosition = default;
    [SerializeField] 
    [AllowNesting] [ShowIf("Fixed")] private RectTransform _groupFixedPosition = default;
    [SerializeField] 
    [Range(-50f, 50f)] [AllowNesting] [Label("X Padding (-50 to 50)")] [HideIf("Fixed")] 
    private float _mousePaddingX = default;
    [SerializeField] 
    [Range(-50f, 50f)] [AllowNesting] [Label("Y Padding (-50 to 50)")] [HideIf("Fixed")] 
    private float _mousePaddingY = default;
    
    [Header("Keyboard and Controller Settings")]
    [SerializeField] 
    [AllowNesting] [Label("Tooltip Preset")] [HideIf("Fixed")] private UseSide _positionToUse = UseSide.ToTheRightOf;
    [SerializeField] 
    [AllowNesting] [Label("Offset Position")] [HideIf("Fixed")] private ToolTipAnchor _keyboardPosition = default;
    [SerializeField] 
    [AllowNesting] [Label("GameObject Marker")] [ShowIf("UseGameObject")] private RectTransform _fixedPosition = default;
    [SerializeField]  
    [Range(-50f, 50f)] [AllowNesting] [Label("X Padding (-50 to 50)")] [HideIf("Fixed")] 
    private float _keyboardPaddingX = default;
    [SerializeField] 
    [Range(-50f, 50f)] [AllowNesting] [Label("Y Padding (-50 to 50)")] [HideIf("Fixed")] 
    private float _keyboardPaddingY = default;
    
    [Header("Other Settings")]
    [SerializeField] 
    [Range(0f, 50f)] private float _screenSafeZone = 10;
    [SerializeField] 
    [Range(0.1f, 5f)] private float _displayTooltipDelay = 1f;
    [SerializeField] 
    [Range(0.1f, 5f)] private float _buildDelay = 1f;
    
    [Header("FadeTween Settings")]
    [SerializeField] 
    [Range(0.1f, 1f)]private float _fadeInTweenTime = 0.2f;
    [SerializeField] 
    [Range(0.1f, 1f)]private float _fadeOutTweenTime = 0.2f;


    public TooltipType ToolTipType => _tooltipType;
    public ToolTipAnchor ToolTipPosition => _toolTipPosition;
    public RectTransform GroupFixedPosition => _groupFixedPosition;
    public float MouseXPadding => _mousePaddingX;
    public float MouseYPadding => _mousePaddingY;
    public UseSide PositionToUse => _positionToUse;
    public ToolTipAnchor KeyboardPosition => _keyboardPosition;
    public RectTransform FixedPosition => _fixedPosition;
    public float KeyboardXPadding => _keyboardPaddingX;
    public float KeyboardYPadding => _keyboardPaddingY;
    public float ScreenSafeZone => _screenSafeZone;
    public float StartDelay => _displayTooltipDelay;
    public float BuildDelay => _buildDelay;
    public float FadeInTime => _fadeInTweenTime;
    public float FadeOutTime => _fadeOutTweenTime;

    public bool Fixed() => _tooltipType == TooltipType.Fixed;
    public bool FollowMouse() => _tooltipType == TooltipType.Follow;
    public bool UseGameObject() => _positionToUse == UseSide.GameObjectAsPosition && _tooltipType != TooltipType.Fixed;

}
