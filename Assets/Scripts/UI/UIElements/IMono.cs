namespace UIElements
{
    public interface IMonoEnable
    {
        void OnEnable();
    }
    public interface IMonoDisable
    {
        void OnDisable();
    }
    public interface IMonoStart
    {
        void OnStart();
    }

    public interface IMonoAwake
    {
        void OnAwake();
    }

    public interface IMonoOnDestroy
    {
        void OnDestroy();
    }
    
    public interface IMono : IMonoAwake, IMonoEnable, IMonoDisable, IMonoStart, IMonoOnDestroy { }
}