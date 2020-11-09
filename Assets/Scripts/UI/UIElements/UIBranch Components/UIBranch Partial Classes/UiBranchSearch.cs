public interface IBranchSearch
{
    UiBranchSearch Find(INode toFind);
    UiBranchSearch DefaultReturn(INode returnDefault);
    INode SearchThisBranchesNodes(INode[] group);
}

public class UiBranchSearch: IBranchSearch
{
    private INode _toFind;
    private INode _default;

    public UiBranchSearch Find(INode toFind)
    {
        _toFind = toFind;
        return this;
    }

    public UiBranchSearch DefaultReturn(INode returnDefault)
    {
        _default = returnDefault;
        return this;
    }

    public INode SearchThisBranchesNodes(INode[] group)
    {
        for (var i = @group.Length - 1; i >= 0; i--)
        {
            if (@group[i] != _toFind) continue;
            return _toFind;
        }
        return _default;
    }
}