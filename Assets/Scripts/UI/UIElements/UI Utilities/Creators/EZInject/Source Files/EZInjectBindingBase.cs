
namespace EZ.Inject
{
    public abstract class EZInjectBindingBase
    {
        protected EZInjectMaster _injectMaster;
        private bool _bound;

        public EZInjectMaster Injector() => _injectMaster;
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
}