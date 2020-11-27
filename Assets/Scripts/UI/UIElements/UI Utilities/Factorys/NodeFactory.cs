using System;

public static class NodeFactory
{
    private static IEJect ieJect = new EJect();
    
    public static INodeBase Factory(ButtonFunction functions, INode node)
    {
        switch (functions)
        {
            case ButtonFunction.Standard:
            {
                return ieJect.WithParams<IStandard>(node);
            }
            case ButtonFunction.ToggleGroup:
            {
                return ieJect.WithParams<ILinkedToggles>(node);
            }
            case ButtonFunction.ToggleNotLinked:
            {
                return ieJect.WithParams<IToggleNotLinked>(node);
            }
            case ButtonFunction.HoverToActivate:
            {
                return ieJect.WithParams<IHoverToActivate>(node);
            }
            case ButtonFunction.CancelOrBack:
            {
                return ieJect.WithParams<ICancelOrBack>(node);
            }
            default:
                throw new Exception("No Node Type Found");
        }
    }
}
