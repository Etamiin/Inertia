using System;

namespace Inertia.Paper
{
    public class PenSystemDuplicateException : Exception
    {
        public override string Message => $"The pen system '{_penType.Name}' can't process on '{_paperType.Name}' because this Type is already being processed by an other pen system.";

        private readonly Type _penType;
        private readonly Type _paperType;

        public PenSystemDuplicateException(Type systemType, Type dataType)
        {
            _penType = systemType;
            _paperType = dataType;
        }
    }
}
