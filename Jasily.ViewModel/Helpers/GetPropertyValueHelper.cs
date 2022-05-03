using System;
using System.Collections.Generic;
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

            if (obj?.GetType().GetProperty(propertyName) is { CanRead: true } prop)
            {
                return prop.GetValue(obj, null);
            }

            return null;
        }
    }
}
