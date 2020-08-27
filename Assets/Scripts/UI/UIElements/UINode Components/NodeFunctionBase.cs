
public abstract class NodeFunctionBase
{

    protected bool _pointerOver;
    protected bool _isHighlighted;
    protected bool _isSelected;
    protected bool _isPressed;

    protected virtual bool CanActivate { get; set; }

    public virtual void OnAwake(UINode node, Setting setting, Actions actions)
    {
        actions._whenPointerOver = SavePointerStatus;
        actions._isHighlighted = SaveHighlighted;
        actions._isSelected = SaveIsSelected;
        actions._isPressed = SaveIsPressed;
    }

    private protected abstract void SavePointerStatus(bool pointerOver);
    private protected abstract void SaveIsSelected(bool isSelected);
    private protected abstract void SaveHighlighted(bool isHighlighted);
    private protected abstract void SaveIsPressed(bool pressed);

}
