using UnityEngine.EventSystems;

public interface IReturnFromEditor
{
    bool CanReturn(bool inMenu, IBranch activeBranch);
}

public class ReturnControlFromEditor : IReturnFromEditor, IEServUser
{
    private InputScheme _scheme;

    public ReturnControlFromEditor() => UseEServLocator();

    public void UseEServLocator() => _scheme = EServ.Locator.Get<InputScheme>(this);

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