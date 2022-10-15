using System;
using System.Collections.Generic;

using System.ComponentModel;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Jasily.ViewModel.Extensions
{
    public static class NotifyPropertyChangedSourceExtensions
    {
        /// <summary>
        /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> with all properties 
        /// that has attribute <see cref="ModelPropertyAttribute"/> 
        /// in this instance.
        /// </summary>
        public static void RefreshProperties(this INotifyPropertyChangedSource source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var mapper = RefreshPropertiesMapper.FromType(source.GetType());

            foreach (var args in mapper.Properties)
            {
                source.NotifyPropertyChanged(source, args);
            }
        }

        /// <summary>
        /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> with all properties 
        /// that has specials group attribute <see cref="ModelPropertyAttribute"/>
        /// in this instance.
        /// </summary>
        public static void RefreshProperties(this INotifyPropertyChangedSource source, int group)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var mapper = RefreshPropertiesMapper.FromType(source.GetType());

            foreach (var args in mapper.GetPropertiesByGroup(group))
            {
                source.NotifyPropertyChanged(source, args);
            }
        }

        internal class RefreshPropertiesMapper
        {
            private static readonly ConcurrentDictionary<Type, RefreshPropertiesMapper> s_Cache
                = new ConcurrentDictionary<Type, RefreshPropertiesMapper>();
            private static readonly Func<Type, RefreshPropertiesMapper> s_Factory = Create;

            public static RefreshPropertiesMapper FromType(Type type) => s_Cache.GetOrAdd(type, s_Factory);

            private static RefreshPropertiesMapper Create(Type type) => new RefreshPropertiesMapper(type);

            RefreshPropertiesMapper(Type type)
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

            internal IReadOnlyList<PropertyChangedEventArgs> GetPropertiesByGroup(int group) =>
                this.GroupedProperties.TryGetValue(group, out var v) ? v : Array.Empty<PropertyChangedEventArgs>();
        }
    }
}
