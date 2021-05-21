namespace EZ.Service
{
    public interface IIsAService
    {
        void AddService();
        void OnRemoveService();
    }

    public interface IServiceUser
    {
        void UseEZServiceLocator();
    }
}
