using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EZ.Events
{
    public class EVentMasterProcessor
    {
        public static Action<TType> Get<TType>(Hashtable events)
        {
            if (!events.ContainsKey(typeof(TType)))
            {
                HandleNoEvent<TType>();
            }
            else
            {
                var temp = (CustomEZEvent<TType>) events[typeof(TType)];
                return temp.RaiseEvent;
            }

            return default;
        }

        public static void Subscribe<TType>(Action<TType> listener, Hashtable events)
        {
            if (!events.ContainsKey(typeof(TType)))
            {
                HandleNoEvent<TType>();
            }
            else
            {
                var eVent = (CustomEZEvent<TType>) events[typeof(TType)];
                eVent.AddListener(listener);
            }
        }

        private static void HandleNoEvent<TType>([CallerMemberName]string from = null)
        {
            Debug.Log($"{typeof(TType)} Event Not Bound in {from} : {Environment.NewLine} Please Bind {typeof(TType)}");
        }

        public static void Unsubscribe<TType>(Action<TType> listener, Hashtable events)
        {
            var eVent = (CustomEZEvent<TType>) events[typeof(TType)];
            eVent.RemoveListener(listener);
        }
    }
}