
using NaughtyAttributes;
using UnityEngine;

public interface IInput
{
    
}
[CreateAssetMenu(menuName = "New Input Scheme - Old", fileName = "Scheme - Old")]
public class InputScheme : ScriptableObject, IInput
{
    [SerializeField] 
    private ControlMethod _mainControlType = ControlMethod.MouseOnly;

    [Header("Pause Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [Label("Nothing to Cancel")]
    private PauseOptionsOnEscape _pauseOptionsOnEscape = PauseOptionsOnEscape.DoNothing;
    [SerializeField] 
    [Label("Pause / Option Button")] [InputAxis]
    private string _pauseOptionButton;
    
    [Header("Home Branch Switch Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField]
    [HideIf("MouseOnly")] [InputAxis] private string _posSwitchButton;
    [SerializeField] 
    [HideIf("MouseOnly")] [InputAxis] private string _negSwitchButton;
    
    [Header("Cancel Settings")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] 
    [InputAxis] private string _cancelButton;
    [Header("In-Game Menu Settings")]
    [SerializeField]
    [DisableIf("MouseOnly")] private InGameSystem _inGameMenuSystem = InGameSystem.Off;
    [SerializeField] 
    private StartInMenu _startGameWhere = StartInMenu.InGameControl;
    [SerializeField] 
    [Label("Switch To/From Game Menus")] [InputAxis] private string _switchToMenusButton;

    //Editor
    private bool MouseOnly() => _mainControlType == ControlMethod.MouseOnly;
}
