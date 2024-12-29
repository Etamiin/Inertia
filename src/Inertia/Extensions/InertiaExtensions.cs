using Inertia;
using System.Collections.Generic;
using System.Reflection;

namespace System
{
    public static class InertiaExtensions
    {
        private readonly static Random Randomizer = new Random();

        internal static T TryInvokeConstructor<T>(this Type type)
        {
            return TryInvokeConstructor<T>(type, BindingFlags.Instance | BindingFlags.Public);
        }
        internal static T TryInvokeConstructor<T>(this Type type, BindingFlags bindingFlags)
        {
            var constructor = type.GetConstructor(bindingFlags, Type.DefaultBinder, Type.EmptyTypes, new ParameterModifier[0]);
            if (constructor is null) throw new ConstructorNotFoundException(type, Type.EmptyTypes);

            return (T)constructor.Invoke(new object[0]);
        }
        internal static T TryInvokeConstructor<T>(this Type type, Type[] parametersTypes, params object[] parameters)
        {
            return TryInvokeConstructor<T>(type, BindingFlags.Instance | BindingFlags.Public, parametersTypes, parameters);
        }
        internal static T TryInvokeConstructor<T>(this Type type, BindingFlags bindingFlags, Type[] parametersTypes, params object[] parameters)
        {
            var constructor = type.GetConstructor(bindingFlags, Type.DefaultBinder, parametersTypes, new ParameterModifier[0]);
            if (constructor is null) throw new ConstructorNotFoundException(type, parametersTypes);

            return (T)constructor.Invoke(parameters);
        }

        public static void Shuffle<T>(this IList<T> collection)
        {
            var iStart = 0;
            T savedValue;

            while (iStart < collection.Count - 1)
            {
                int iRand = Randomizer.Next(iStart, collection.Count);
                savedValue = collection[iStart];
                collection[iStart++] = collection[iRand];
                collection[iRand] = savedValue;
            }
        }
        public static void ThrowIfDisposable(this IDisposable disposable, bool isDisposed)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(disposable.GetType().Name);
            }
        }
    }
}