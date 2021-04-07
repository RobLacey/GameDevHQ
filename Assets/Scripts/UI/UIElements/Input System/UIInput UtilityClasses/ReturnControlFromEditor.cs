using UnityEngine.EventSystems;

public interface IReturnFromEditor
{
    bool CanReturn(bool inMenu, IBranch activeBranch);
}

public class ReturnControlFromEditor : IReturnFromEditor
{
    private readonly InputScheme _scheme;

    public ReturnControlFromEditor(IInput uiInput)
    {
        _scheme = uiInput.ReturnScheme;
    }

    public bool CanReturn(bool inMenu, IBranch activeBranch)
    {
        if(!inMenu) return false;
        
        if (_scheme.LeftMouseClicked 
            && _scheme.ControlType == ControlMethod.KeysOrControllerOnly)
        {
            EventSystem.current.SetSelectedGameObject(activeBranch.LastSelected.ReturnGameObject);
            return true;
        }
        return false;
    }
}