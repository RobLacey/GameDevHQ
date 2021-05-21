    namespace EZ.Events
    {
        public interface IEZEventDispatcher
        {
            void FetchEvents();
        }

        public interface IEZEventUser
        {
            void ObserveEvents();
        }
    }