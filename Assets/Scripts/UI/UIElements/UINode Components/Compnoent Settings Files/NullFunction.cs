public class NullFunction : NodeFunctionBase
{
    protected override void OnAwake(UiActions uiActions) { }
    public override void OnDisable() { }
    protected override bool CanBeHighlighted() { return false; }
    protected override bool CanBePressed() { return false; }
    protected override bool FunctionNotActive() { return false; }
    protected override void SavePointerStatus(bool pointerOver) { }
    private protected override void ProcessPress() { }
    private protected override void ProcessDisabled() { }
}