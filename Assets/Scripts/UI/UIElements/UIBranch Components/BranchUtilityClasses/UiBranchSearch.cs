public class NodeSearch
{
    private INode _toFind;
    private INode _default;
    
    public static NodeSearch Search { get; } = new NodeSearch();
    
    public NodeSearch Find(INode toFind)
    {
        _toFind = toFind;
        return Search;
    }

    public NodeSearch DefaultReturn(INode returnDefault)
    {
        _default = returnDefault;
        return Search;
    }

    public INode RunOn(INode[] group)
    {
        for (var i = group.Length - 1; i >= 0; i--)
        {
            if (group[i] != _toFind) continue;
            return _toFind;
        }
        return _default;
    }
}