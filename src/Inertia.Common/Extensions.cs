using Inertia;
using System.Collections.Generic;
using System.Reflection;

namespace System
{
    public static class Extensions
    {
        private static readonly Random Randomizer = new Random();

        public static T InvokeConstructor<T>(this Type type)
        {
            return InvokeConstructor<T>(type, BindingFlags.Instance | BindingFlags.Public);
        }
        public static object InvokeConstructor(this Type type)
        {
            return InvokeConstructor(type, BindingFlags.Instance | BindingFlags.Public);
        }
        public static T InvokeConstructor<T>(this Type type, BindingFlags bindingFlags)
        {
            return (T)InvokeConstructor(type, bindingFlags);
        }
        public static object InvokeConstructor(this Type type, BindingFlags bindingFlags)
        {
            var constructor = type.GetConstructor(bindingFlags, Type.DefaultBinder, Type.EmptyTypes, Array.Empty<ParameterModifier>());
            if (constructor is null)
            {
                throw new ConstructorNotFoundException(type, Type.EmptyTypes);
            }

            return constructor.Invoke(new object[0]);
        }
        public static T InvokeConstructor<T>(this Type type, Type[] parametersTypes, params object[] parameters)
        {
            return InvokeConstructor<T>(type, BindingFlags.Instance | BindingFlags.Public, parametersTypes, parameters);
        }
        public static object InvokeConstructor(this Type type, Type[] parametersTypes, params object[] parameters)
        {
            return InvokeConstructor(type, BindingFlags.Instance | BindingFlags.Public, parametersTypes, parameters);
        }
        public static T InvokeConstructor<T>(this Type type, BindingFlags bindingFlags, Type[] parametersTypes, params object[] parameters)
        {
            return (T)InvokeConstructor(type, bindingFlags, parametersTypes, parameters);
        }
        public static object InvokeConstructor(this Type type, BindingFlags bindingFlags, Type[] parametersTypes, params object[] parameters)
        {
            var constructor = type.GetConstructor(bindingFlags, Type.DefaultBinder, parametersTypes, new ParameterModifier[0]);
            if (constructor is null)
            {
                throw new ConstructorNotFoundException(type, parametersTypes);
            }

            return constructor.Invoke(parameters);
        }

        public static void Shuffle<T>(this IList<T> collection)
        {
            var startIndex = 0;
            T savedValue;

            while (startIndex < collection.Count - 1)
            {
                int iRand = Randomizer.Next(startIndex, collection.Count);
                savedValue = collection[startIndex];
                collection[startIndex++] = collection[iRand];
                collection[iRand] = savedValue;
            }
        }
    }
}