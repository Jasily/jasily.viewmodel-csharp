using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace Jasily.ViewModel.Helpers
{
    public static class GetPropertyValueHelper
    {
        /// <summary>
        /// User can special a callback to get property value if they want, for example, FastMember or compiled expression.
        /// </summary>
        public static Func<object, string, object> GetPropertyValueCallback { get; set; }

        /// <summary>
        /// Get property value from <paramref name="obj"/>.
        /// 
        /// If <see cref="GetPropertyValueCallback"/> is null, use <see cref="PropertyInfo.GetValue(object)"/>.
        /// The implementation may change in the future.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (propertyName is null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (GetPropertyValueCallback is { } callback)
            {
                return callback(obj, propertyName);
            }

            return GetPropertyValueWithReflection(obj, propertyName);
        }

        private static object GetPropertyValueWithReflection(object obj, string propertyName)
        {
            if (obj?.GetType().GetProperty(propertyName) is { CanRead: true } property)
            {
                return property.GetValue(obj, null);
            }

            return null;
        }

        /// <summary>
        /// A helper method for <see cref="GetPropertyValueCallback"/>.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Func<object, string, object> CreateGetPropertyValueCallbackWithCreateDelegate()
        {
            var createDelegateForGetterMethod = 
                typeof(GetPropertyValueHelper).GetMethod(nameof(CreateDelegateForGetter), BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(createDelegateForGetterMethod != null);

            Func<object, object> alwaysNull = _ => null;

            Func<(Type, string), Func<object, object>> factory = key =>
            {
                var (type, propertyName) = key;
                if (type.GetProperty(propertyName) is { CanRead: true } property)
                {
                    return (Func<object, object>) createDelegateForGetterMethod.MakeGenericMethod(type, property.PropertyType)
                        .Invoke(null, new [] { property.GetGetMethod() } );
                }
                return alwaysNull;
            };

            var store = new ConcurrentDictionary<(Type, string), Func<object, object>>();

            return (obj, propertyName) =>
            {
                if (propertyName is null) 
                    throw new ArgumentNullException(nameof(propertyName));

                if (obj is null)
                    return null;

                return store.GetOrAdd((obj.GetType(), propertyName), factory).Invoke(obj);
            };
        }

        private static Func<object, object> CreateDelegateForGetter<TThis, TProperty>(MethodInfo getter)
        {
            var func = (Func<TThis, TProperty>)getter.CreateDelegate(typeof(Func<TThis, TProperty>));
            return o => func((TThis)o);
        }
    }
}
