using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Jasily.ViewModel.Extensions;
using JetBrains.Annotations;

namespace Jasily.ViewModel
{
    public class NotifyPropertyChangedObject : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, CallerMemberName] string propertyName = "")
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            this.PropertyChanged?.Invoke(this, propertyName);
        }

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull] params string[] propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            this.PropertyChanged?.Invoke(this, propertyNames);
        }

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull] IEnumerable<string> propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            this.PropertyChanged?.Invoke(this, propertyNames);
        }

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull] params PropertyChangedEventArgs[] eventArgs)
        {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
            this.PropertyChanged?.Invoke(this, eventArgs);
        }

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull] IEnumerable<PropertyChangedEventArgs> propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            this.PropertyChanged?.Invoke(this, propertyNames);
        }

        protected void ClearPropertyChangedInvocationList() => this.PropertyChanged = null;

        public void Dispose()
        {
            this.PropertyChanged = null;
        }
    }
}
