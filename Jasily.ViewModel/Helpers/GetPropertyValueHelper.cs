using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace Jasily.ViewModel.Helpers
{
    public static class GetPropertyValueHelper
    {
        /// <summary>
        /// Get property value from <paramref name="obj"/>.
        /// If the property does not exists or it does not has getter, return null.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is null.</exception>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (propertyName is null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (obj is null)
            {
                return null;
            }

            return s_Store.GetOrAdd((obj.GetType(), propertyName), s_GetPropertyValueFactory).Invoke(obj);
        }

        private static readonly MethodInfo s_CreateDelegateForGetterMethodInfo =
            typeof(GetPropertyValueHelper).GetMethod(nameof(CreateDelegateForGetter), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Func<object, object> s_AlwaysNull = _ => null;
        private static readonly Func<(Type, string), Func<object, object>> s_GetPropertyValueFactory = key =>
        {
            var (type, propertyName) = key;
            if (type.GetProperty(propertyName) is { CanRead: true } property)
            {
                return (Func<object, object>)s_CreateDelegateForGetterMethodInfo.MakeGenericMethod(type, property.PropertyType)
                    .Invoke(null, new[] { property.GetGetMethod() });
            }
            return s_AlwaysNull;
        };
        private static readonly ConcurrentDictionary<(Type, string), Func<object, object>> s_Store = 
            new ConcurrentDictionary<(Type, string), Func<object, object>>();

        private static Func<object, object> CreateDelegateForGetter<TThis, TProperty>(MethodInfo getter)
        {
            var func = (Func<TThis, TProperty>)Delegate.CreateDelegate(typeof(Func<TThis, TProperty>), getter);
            return o => func((TThis)o);
        }
    }
}
