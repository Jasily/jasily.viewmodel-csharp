using System;
using System.Collections.Generic;
using System.ComponentModel;

using JetBrains.Annotations;

namespace Jasily.ViewModel.Extensions
{
    public static partial class NotifyPropertyChangedExtensions
    {
        public static void Invoke([NotNull] this PropertyChangedEventHandler handler, object sender,
            string propertyName)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            handler.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }

        public static void Invoke([NotNull] this PropertyChangedEventHandler handler, object sender,
            [NotNull] params string[] propertyNames)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            foreach (var propertyName in propertyNames)
            {
                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static void Invoke([NotNull] this PropertyChangedEventHandler handler, object sender,
            [NotNull] IEnumerable<string> propertyNames)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            foreach (var propertyName in propertyNames)
            {
                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static void Invoke([NotNull] this PropertyChangedEventHandler handler, object sender,
            [NotNull] params PropertyChangedEventArgs[] eventArgs)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
            foreach (var property in eventArgs)
            {
                handler(sender, property);
            }
        }

        public static void Invoke([NotNull] this PropertyChangedEventHandler handler, object sender,
            [NotNull] IEnumerable<PropertyChangedEventArgs> eventArgs)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
            foreach (var property in eventArgs)
            {
                handler(sender, property);
            }
        }
    }
}
