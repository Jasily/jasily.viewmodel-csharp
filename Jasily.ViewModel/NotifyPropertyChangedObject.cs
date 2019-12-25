using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Jasily.ViewModel.Extensions;
using JetBrains.Annotations;

namespace Jasily.ViewModel
{
    /// <summary>
    /// The base class for interface <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public class NotifyPropertyChangedObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, CallerMemberName] string propertyName = "")
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            this.PropertyChanged?.Invoke(this, propertyName);
        }

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, ItemNotNull] params string[] propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            this.PropertyChanged?.Invoke(this, propertyNames);
        }

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, ItemNotNull] IEnumerable<string> propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            this.PropertyChanged?.Invoke(this, propertyNames);
        }

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, ItemNotNull] params PropertyChangedEventArgs[] eventArgs)
        {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
            this.PropertyChanged?.Invoke(this, eventArgs);
        }

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, ItemNotNull] IEnumerable<PropertyChangedEventArgs> propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            this.PropertyChanged?.Invoke(this, propertyNames);
        }

        [NotifyPropertyChangedInvocator]
        protected bool ChangeModelProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue)) return false;
            field = newValue;
            this.NotifyPropertyChanged(propertyName);
            return true;
        }

        protected void ClearPropertyChangedInvocationList() => this.PropertyChanged = null;
    }
}
