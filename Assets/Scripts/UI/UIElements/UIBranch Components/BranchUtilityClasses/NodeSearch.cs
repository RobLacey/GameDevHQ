using UnityEngine;

public interface INodeSearch
{
    INodeSearch DefaultReturn(INode returnDefault);
    INode RunOn(INode[] group);
}

public class NodeSearch : INodeSearch
{
    private static INode toFind;
    private INode _default;
    
    private static INodeSearch Search { get; } = new InjectClass().NoParams<INodeSearch>();
    
    public static INodeSearch Find(INode nodeToFind)
    {
        toFind = nodeToFind;
        return Search;
    }

    public INodeSearch DefaultReturn(INode returnDefault)
    {
        _default = returnDefault;
        return Search;
    }

    public INode RunOn(INode[] group)
    {
        for (var i = group.Length - 1; i >= 0; i--)
        {
            if (group[i] != toFind) continue;
            return toFind;
        }
        return _default;
    }
}