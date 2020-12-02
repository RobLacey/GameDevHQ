
public abstract class BindBase
{
    protected EJectMaster _eJectMaster;
    private bool _bound;

    public EJectMaster EJectClass() => _eJectMaster;
    protected abstract void BindAllObjects();
    
    protected bool CheckIfAlreadyBound()
    {
        if (!_bound)
        {
            _bound = true;
            return false;
        }
        return true;
    }

}