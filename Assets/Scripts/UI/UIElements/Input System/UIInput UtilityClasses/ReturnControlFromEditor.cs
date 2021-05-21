using EZ.Service;
using UnityEngine.EventSystems;

public interface IReturnFromEditor
{
    bool CanReturn(bool inMenu, IBranch activeBranch);
}

public class ReturnControlFromEditor : IReturnFromEditor, IServiceUser
{
    private InputScheme _scheme;

    public ReturnControlFromEditor() => UseEZServiceLocator();

    public void UseEZServiceLocator() => _scheme = EZService.Locator.Get<InputScheme>(this);

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