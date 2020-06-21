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
    internal abstract class RefreshPropertiesMapper
    {
        private static readonly ConcurrentDictionary<Type, RefreshPropertiesMapper> Store
            = new ConcurrentDictionary<Type, RefreshPropertiesMapper>();
        private static readonly Func<Type, RefreshPropertiesMapper> Factory = Create;

        public static RefreshPropertiesMapper FromType(Type type) => Store.GetOrAdd(type, Factory);

        private static RefreshPropertiesMapper Create(Type type)
        {
            return (RefreshPropertiesMapper)Activator.CreateInstance(
                typeof(RefreshPropertiesMapper<>).MakeGenericType(type));
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
                .ToDictionary(z => z.Key, z => (IReadOnlyCollection<PropertyChangedEventArgs>)z.Select(x => x.Item2).ToArray());

            var callbacks = from method in type.GetRuntimeMethods()
                            let attr = method.GetCustomAttribute<OnNotifyChangedAttribute>()
                            where attr != null
                            where !method.IsGenericMethod
                            where method.ReturnType == typeof(void)
                            where method.GetParameters().Length == 0
                            select (attr, method);

            this.OnNotifyChangedCallbacks = callbacks.GroupBy(z => z.attr.PropertyName)
                .ToDictionary(
                    z => z.Key,
                    z => (IReadOnlyCollection<Lazy<Action<object>>>) z.Select(x => {
                        var m = x.method;
                        return new Lazy<Action<object>>(
                            () => this.CreateParameterlessAction(m)
                        );
                    }).ToArray());
        }

        internal IReadOnlyCollection<PropertyChangedEventArgs> Properties { get; }

        internal IDictionary<int, IReadOnlyCollection<PropertyChangedEventArgs>> GroupedProperties { get; }

        internal IDictionary<string, IReadOnlyCollection<Lazy<Action<object>>>> OnNotifyChangedCallbacks { get; }

        internal PropertiesChangedData GetPropertiesChangedData() => new PropertiesChangedData(this, this.Properties);

        internal PropertiesChangedData GetPropertiesChangedData(int group)
        {
            return new PropertiesChangedData(this, 
                this.GroupedProperties.TryGetValue(group, out var v) ? v : Array.Empty<PropertyChangedEventArgs>());
        }

        protected abstract Action<object> CreateParameterlessAction(MethodInfo methodInfo);
    }

    internal sealed class RefreshPropertiesMapper<T> : RefreshPropertiesMapper
    {
        internal RefreshPropertiesMapper() : base(typeof(T))
        {
        }

        protected override Action<object> CreateParameterlessAction(MethodInfo methodInfo)
        {
            var callback = (Action<T>) Delegate.CreateDelegate(typeof(Action<T>), methodInfo);
            return obj => callback((T)obj);
        }
    }
}
