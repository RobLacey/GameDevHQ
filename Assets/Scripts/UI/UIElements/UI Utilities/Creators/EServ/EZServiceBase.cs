
namespace EZ.Service
{
    public interface IEZServiceBase
    {
        void AddNew<T>(IIsAService service);
        T Get<T>(IServiceUser ieServUser);
        T LateGet<T>();
        IEZServiceBase Unlock();
        void Purge();
    }

    public abstract class EZServiceBase<TLocator> : IEZServiceBase where TLocator : new()
    {
        public static TLocator Locator { get; } = new TLocator();
        protected EZServiceMaster Service { get; set; }

        public abstract void AddNew<T>(IIsAService service);
        public abstract T Get<T>(IServiceUser ieServUser);
        public abstract T LateGet<T>();
        public abstract IEZServiceBase Unlock();
        public abstract void Purge();
    }
}