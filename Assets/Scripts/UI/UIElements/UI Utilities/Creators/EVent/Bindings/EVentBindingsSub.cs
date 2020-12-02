public class EVentBindingsSub : BindBase
{
    public EVentBindingsSub()
    {
        //_eVentMaster = new EVentMaster();
        BindAllObjects();
    }

    protected sealed override void BindAllObjects()
    {
        if (CheckIfAlreadyBound()) return;

        //Node

    }

}