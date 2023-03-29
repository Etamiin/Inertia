using System;

namespace Inertia.Scriptable
{
    public class ScriptComponentSystemDuplicate : Exception
    {
        public override string Message => $"The component system '{_systemType.Name}' can't process on '{_dataType.Name}' component data because it is already being processed by an other component system.";

        private readonly Type _systemType;
        private readonly Type _dataType;

        public ScriptComponentSystemDuplicate(Type systemType, Type dataType)
        {
            _systemType = systemType;
            _dataType = dataType;
        }
    }
}
