using System;

public static class NodeFactory
{
    private static IInjectClass injectClass = new IoC();
    
    public static INodeBase Factory(ButtonFunction functions, INode node)
    {
        switch (functions)
        {
            case ButtonFunction.Standard:
            {
                return injectClass.WithParams<IStandard>(node);
            }
            case ButtonFunction.ToggleGroup:
            {
                return injectClass.WithParams<ILinkedToggles>(node);
            }
            case ButtonFunction.ToggleNotLinked:
            {
                return injectClass.WithParams<IToggleNotLinked>(node);
            }
            case ButtonFunction.HoverToActivate:
            {
                return injectClass.WithParams<IHoverToActivate>(node);
            }
            case ButtonFunction.CancelOrBack:
            {
                return injectClass.WithParams<ICancelOrBack>(node);
            }
            default:
                throw new Exception("No Node Type Found");
        }
    }
}
