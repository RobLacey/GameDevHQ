using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EZ.Events
{
    public interface ICloseEvent
    {
        void ClearUp();
    }

    public class CustomEZEvent<TType> : ICloseEvent
    {
        private event Action<TType> Raise;
        private int? _currentScene;
        private bool _autoRemove;

        public void RaiseEvent(TType args) => Raise?.Invoke(args);

        public CustomEZEvent<TType> AutoRemove()
        {
            _autoRemove = true;
            return this;
        }
    
        public void AddListener(Action<TType> newEvent)
        {
            Raise -= newEvent;
            Raise += newEvent;
        }

        public void RemoveListener(Action<TType> newEvent) => Raise -= newEvent;

        public void ClearUp()
        {
            if(Raise.IsNull() || !_autoRemove) return;
            Raise = null;
        }
    }

}