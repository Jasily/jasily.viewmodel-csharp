using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Jasily.ViewModel.Internal
{
    internal class RefreshPropertiesMapper
    {
        private static readonly ConcurrentDictionary<Type, RefreshPropertiesMapper> Store
            = new ConcurrentDictionary<Type, RefreshPropertiesMapper>();
        private static readonly Func<Type, RefreshPropertiesMapper> Factory = type => new RefreshPropertiesMapper(type);
        private readonly PropertyChangedEventArgs[] _properties;

        public static RefreshPropertiesMapper FromType(Type type) => Store.GetOrAdd(type, Factory);

        internal RefreshPropertiesMapper([NotNull] Type type)
        {
            this._properties = (
                from property in type.GetRuntimeProperties()
                let attr = property.GetCustomAttribute<ModelPropertyAttribute>()
                where attr != null
                orderby attr.Order
                select new PropertyChangedEventArgs(property.Name)
                ).ToArray();
        }

        internal IReadOnlyCollection<PropertyChangedEventArgs> GetProperties() => this._properties;
    }
}
