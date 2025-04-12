using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Inertia.IO
{
    internal sealed class SerializedPropertyMetadata
    {
        private Func<object, object> _getValue;
        private Action<object, object> _setValue;

        internal SerializedPropertyMetadata(PropertyInfo property)
        {
            PropertyType = property.PropertyType;

            CreateAndCompileGetSetExpressions(property);
        }

        internal Type PropertyType { get; }

        internal object GetValue(object instance)
        {
            return _getValue(instance);
        }
        internal void SetValue(object instance, object value)
        {
            _setValue(instance, value);
        }

        private void CreateAndCompileGetSetExpressions(PropertyInfo property)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var instanceCast = Expression.Convert(instanceParameter, property.DeclaringType);
            var propertyAccess = Expression.Property(instanceCast, property);
            var convertResult = Expression.Convert(propertyAccess, typeof(object));
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var valueCast = Expression.Convert(valueParameter, property.PropertyType);
            var assign = Expression.Assign(propertyAccess, valueCast);

            _getValue = Expression.Lambda<Func<object, object>>(convertResult, instanceParameter).Compile();
            _setValue = Expression.Lambda<Action<object, object>>(assign, instanceParameter, valueParameter).Compile();
        }
    }
}
