using System.Collections.Generic;

public class ScreenData
{
    public readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
    public UINode _lastHighlighted;
    public UINode _lastSelected;
    public bool  _wasInTheMenu;
}
