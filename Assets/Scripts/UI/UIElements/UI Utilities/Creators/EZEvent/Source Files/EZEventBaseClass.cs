using System;
using System.Collections;
using System.Collections.Generic;

namespace EZ.Events
{
    public abstract class EZEventBaseClass<TEVent> where TEVent : new()
    {
        public void Initialise(IEZEventBindingBase bindings)
        {
            EVentsList = bindings.Events;
            TypeList = bindings.PurgeList;
        }

        //Properties
        public static TEVent Do { get; } = new TEVent();
        private Hashtable EVentsList { get; set; } = new Hashtable();
        private List<ICloseEvent> TypeList { get; set; } = new List<ICloseEvent>();

        //Main
        public Action<T> Fetch<T>() => EVentMasterProcessor.Get<T>(EVentsList);
        public void Subscribe<T>(Action<T> listener) => EVentMasterProcessor.Subscribe(listener, EVentsList);

        public void Unsubscribe<T>(Action<T> listener)
        {
            if (EVentsList.Count == 0) return;
            EVentMasterProcessor.Unsubscribe(listener, EVentsList);
        }

        public void Purge()
        {
            foreach (var type in TypeList)
            {
                type.ClearUp();
            }

            EVentsList.Clear();
            TypeList.Clear();
        }

    }
}