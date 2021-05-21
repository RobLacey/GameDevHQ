using System;
using System.Collections;
using System.Collections.Generic;

namespace EZ.Inject
{
    public class EZInjectMaster
    {
        private readonly Hashtable _classes = new Hashtable();
        private readonly List<Type> _classesWithParameters = new List<Type>();
        private Type _instanceType;
        private Type _interfaceType;
    
        public EZInjectMaster Bind<T>()
        {
            _instanceType = typeof(T);
            return this;
        }

        public EZInjectMaster To<T>()
        {
            _interfaceType = typeof(T);
            if(_classes.ContainsKey(typeof(T)))
                ThrowParameterException("Class Already Has Key : ", typeof(T));

            _classes.Add(_interfaceType, _instanceType);
            return this;
        }

        public void WithParameters() => _classesWithParameters.Add(_interfaceType);

        public T Get<T>(IParameters args = null)
        {
            var typeOfNewClass = typeof(T);
            var newClass = _classes[typeOfNewClass];
            var hasParams = _classesWithParameters.Contains(typeOfNewClass);
        
            return args is null
                ? NoParameters<T>(typeOfNewClass, newClass, hasParams)
                : WithParameters<T>(args, typeOfNewClass, newClass, hasParams);
        }

        private T WithParameters<T>(IParameters args, Type typeOfNewClass, object newClass, bool hasParams)
        {
            if (!hasParams)
                ThrowParameterException($"Class DOESN'T HAVE Parameters or no Binding", typeOfNewClass);
            return (T) System.Activator.CreateInstance((Type) newClass, args);
        }

        private T NoParameters<T>(Type typeOfNewClass, object newClass, bool hasParams)
        {
            if (hasParams)
                ThrowParameterException("Constructor HAS Parameters", typeOfNewClass);

            return (T) System.Activator.CreateInstance((Type) newClass);
        }

        private void ThrowParameterException(string message, Type typeOfNewClass)
        {
            throw new Exception($"{message} : {typeOfNewClass}");
        }
    }
}