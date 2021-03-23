public partial class GOUIController
{

    private const string UseSafeList = nameof(SafeList);
    private bool SafeList() => _cancelWhen != CancelWhen.EscapeKeyOnly;

    private const string ClearHeader =
        "Clear In Game UI Settings (Escape Key ALWAYS Active)";

    private const string SafeNodeInfo =
        "Nodes in this list won't cause the InGameUI to perform a cancel operation. " +
        "Good for in game object settings menus etc";
}