using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Jasily.ViewModel.Internal
{
    internal static class RefreshPropertiesMapperProvider
    {
        private static readonly ConcurrentDictionary<Type, RefreshPropertiesMapper> Store 
            = new ConcurrentDictionary<Type, RefreshPropertiesMapper>();
        private static readonly Func<Type, RefreshPropertiesMapper> Factory = Create;

        public static RefreshPropertiesMapper FromType(Type type) => Store.GetOrAdd(type, Factory);

        private static RefreshPropertiesMapper Create(Type type) => new RefreshPropertiesMapper(type);
    }
}
