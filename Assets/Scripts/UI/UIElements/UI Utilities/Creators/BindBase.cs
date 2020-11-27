public abstract class BindBase
{
    private bool _bound;

    protected BindBase()
    {
        BindAllObjects();
    }
    
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