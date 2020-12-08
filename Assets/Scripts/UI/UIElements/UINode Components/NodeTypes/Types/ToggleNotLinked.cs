
public interface  IToggleNotLinked : INodeBase { }

public class ToggleNotLinked : NodeBase, IToggleNotLinked
{
    public ToggleNotLinked(INode node) : base(node) { }
}
