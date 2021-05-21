using System.Collections;
using System.Collections.Generic;

namespace EZ.Events
{
    public interface IEZEventBindingBase
    {
        Hashtable Events { get; }
        List<ICloseEvent> PurgeList { get; }
    }

    public abstract class EZEventBindingsBase : IEZEventBindingBase
    {
        protected EZEventBindingsBase() => EventsToBind();

        //Properties
        private bool AutoRemoveEVent { get; set; }
        public Hashtable Events { get; } = new Hashtable();
        public List<ICloseEvent> PurgeList { get; } = new List<ICloseEvent>();

        //Main
        protected abstract void EventsToBind();

        public void CreateEvent<TType>()
        {
            var newEvent = new CustomEZEvent<TType>();
            Events.Add(typeof(TType), AutoRemoveEVent ? newEvent.AutoRemove() 
                           : newEvent);
            PurgeList.Add(newEvent);
            AutoRemoveEVent = false;
        }

        protected EZEventBindingsBase AutoRemove()
        {
            AutoRemoveEVent = true;
            return this;
        }
    }
}