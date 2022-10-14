using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime;

using JetBrains.Annotations;

namespace Jasily.ViewModel.Internal
{
    internal class RefreshPropertiesMapper
    {
        private static readonly ConcurrentDictionary<Type, RefreshPropertiesMapper> Store
            = new ConcurrentDictionary<Type, RefreshPropertiesMapper>();
        private static readonly Func<Type, RefreshPropertiesMapper> Factory = Create;

        public static RefreshPropertiesMapper FromType(Type type) => Store.GetOrAdd(type, Factory);

        private static RefreshPropertiesMapper Create(Type type)
        {
            return new RefreshPropertiesMapper(type);
        }

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
                .ToDictionary(z => z.Key, z => (IReadOnlyList<PropertyChangedEventArgs>)z.Select(x => x.Item2).ToArray());
        }

        internal IReadOnlyList<PropertyChangedEventArgs> Properties { get; }

        internal IDictionary<int, IReadOnlyList<PropertyChangedEventArgs>> GroupedProperties { get; }

        internal IReadOnlyList<PropertyChangedEventArgs> GetProperties() => this.Properties;

        internal IReadOnlyList<PropertyChangedEventArgs> GetPropertiesByGroup(int group) =>
            this.GroupedProperties.TryGetValue(group, out var v) ? v : Array.Empty<PropertyChangedEventArgs>();
    }
}
