
public interface IEServ : IEServBase { }

public class EServ : EServBase<EServ>, IEServ
{
    protected override EServMaster Service { get; } = new EServMaster();

    public override void AddNew<T>(T service) => Service.AddNew(service);

    public override T Get<T>(IEServUser ieServUser) => Service.Get<T>(ieServUser);

    public override IEServBase Unlock()
    {
        Service.Unlock();
        return this;
    }
}
