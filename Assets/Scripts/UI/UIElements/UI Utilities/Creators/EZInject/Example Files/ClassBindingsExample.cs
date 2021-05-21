using EZ.Inject;

public class ClassBindingsExample : EZInjectBindingBase
{
    public ClassBindingsExample()
    {
        _injectMaster = new EZInjectMaster();
        BindAllObjects();
    }

    protected sealed override void BindAllObjects()
    {
        if(CheckIfAlreadyBound()) return;

        // ****Add Classes Here****

    }
}