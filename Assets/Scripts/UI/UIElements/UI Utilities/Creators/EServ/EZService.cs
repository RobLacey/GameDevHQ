
namespace EZ.Service
{
    public interface IEZService : IEZServiceBase { }
    

    public class EZService : EZServiceBase<EZService>, IEZService
    {
        public void Initialise() => Service = new EZServiceMaster();

        public override void AddNew<T>(IIsAService service) => Service.AddNew<T>(service);

        public override T Get<T>(IServiceUser ieServUser) => Service.Get<T>(ieServUser);
        public override T LateGet<T>() => Service.Get<T>();

        public override IEZServiceBase Unlock()
        {
            Service.Unlock();
            return this;
        }

        public override void Purge()
        {
            Service.CleanUpServices();
            Service = null;
        }
    }
}