using System;

public static class NodeFactory
{
    public static INodeBase Factory(ButtonFunction funcs, UINode node)
    {
        switch (funcs)
        {
            case ButtonFunction.Standard:
                return new Standard(node);
            case ButtonFunction.ToggleGroup:
                return new UIToggles(node);
            case ButtonFunction.ToggleNotLinked:
                return new ToggleNotLinked(node);
            case ButtonFunction.HoverToActivate:
                return new HoverToActivate(node);
            case ButtonFunction.CancelOrBack:
                return new CancelOrBackButton(node);
            default:
                throw new Exception("No Node Type Found");
        }
    }
}
