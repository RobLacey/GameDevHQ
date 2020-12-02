public class ClassBindingsSub : BindBase
{
    public ClassBindingsSub()
    {
        _eJectMaster = new EJectMaster();
        BindAllObjects();
    }

    protected sealed override void BindAllObjects()
    {
        if(CheckIfAlreadyBound()) return;

        // ****Add Classes Here****

    }
}