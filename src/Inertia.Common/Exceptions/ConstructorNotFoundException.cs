using System;
using System.Linq;

namespace Inertia
{
    public class ConstructorNotFoundException : Exception
    {
        public ConstructorNotFoundException(Type ownerType, Type[] parametersType) : base(BuildMessage(ownerType, parametersType))
        {
        }

        private static string BuildMessage(Type ownerType, Type[] parametersType)
        {
            if (parametersType.Length == 0) return $"The type '{ownerType.Name}' must contain a constructor without parameters.";

            var typesStr = string.Join(", ", parametersType.Select((type) => type.Name));
            return $"The type '{ownerType.Name}' must contain a constructor with parameters ({typesStr}).";
        }
    }
}
