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

        public static RefreshPropertiesMapper FromType(Type type) => Store.GetOrAdd(type, Factory);

        internal RefreshPropertiesMapper([NotNull] Type type)
        {
            var attributes = from property in type.GetRuntimeProperties()
                             let attr = property.GetCustomAttribute<ModelPropertyAttribute>()
                             where attr != null
                             orderby attr.Order
                             select (attr, new PropertyChangedEventArgs(property.Name));

            this.Properties = attributes
                .Select(z => z.Item2)
                .ToArray();

            this.GroupedProperties = attributes
                .GroupBy(z => z.attr.Group)
                .ToDictionary(z => z.Key, z => (IReadOnlyCollection<PropertyChangedEventArgs>)z.Select(x => x.Item2).ToArray());
        }

        internal IReadOnlyCollection<PropertyChangedEventArgs> Properties { get; }

        internal IDictionary<int, IReadOnlyCollection<PropertyChangedEventArgs>> GroupedProperties { get; }

        internal IReadOnlyCollection<PropertyChangedEventArgs> GetProperties(int group)
        {
            return this.GroupedProperties.TryGetValue(group, out var v) ? v : Array.Empty<PropertyChangedEventArgs>();
        }
    }
}
