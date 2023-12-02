using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public class InjectionDependencyException : Exception
    {
        public override string Message => $"Injection dependency failed, type '{_instanceType.Name}' require '{_interfaceType.Name}' interface.";

        private Type _interfaceType, _instanceType;

        internal InjectionDependencyException(Type interfaceType, Type instanceType)
        {
            _interfaceType = interfaceType;
            _instanceType = instanceType;
        }
    }
}
