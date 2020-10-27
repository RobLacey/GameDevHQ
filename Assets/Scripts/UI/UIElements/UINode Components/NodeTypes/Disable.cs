public class Disable : IDisable
{
    private readonly UINode _uiNode;
    private bool _isDisabled;

    public Disable(UINode uiNode) => _uiNode = uiNode;

    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            _isDisabled = value;
            if (_isDisabled)
                _uiNode.SetSelectedStatus(false, _uiNode.DoPress);
        }
    }

    public bool NodeIsDisabled()
    {
        if (!IsDisabled) return false;
        _uiNode.Navigation.MoveToNextFreeNode();
        return true;
    }
}
