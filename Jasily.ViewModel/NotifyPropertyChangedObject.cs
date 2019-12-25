using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentNullException">throw when <paramref name="propertyName"/> is null.</exception>
        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, CallerMemberName] string propertyName = "")
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            this.PropertyChanged?.Invoke(this, propertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyNames"></param>
        /// <exception cref="ArgumentNullException">throw when <paramref name="propertyNames"/> is null.</exception>
        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, ItemNotNull] params string[] propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            this.PropertyChanged?.Invoke(this, propertyNames);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyNames"></param>
        /// <exception cref="ArgumentNullException">throw when <paramref name="propertyNames"/> is null.</exception>
        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, ItemNotNull] IEnumerable<string> propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            this.PropertyChanged?.Invoke(this, propertyNames);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <exception cref="ArgumentNullException">throw when <paramref name="eventArgs"/> is null.</exception>
        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, ItemNotNull] params PropertyChangedEventArgs[] eventArgs)
        {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
            this.PropertyChanged?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyNames"></param>
        /// <exception cref="ArgumentNullException">throw when <paramref name="propertyNames"/> is null.</exception>
        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, ItemNotNull] IEnumerable<PropertyChangedEventArgs> propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException(nameof(propertyNames));
            this.PropertyChanged?.Invoke(this, propertyNames);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">throw when <paramref name="propertyName"/> is null.</exception>
        [NotifyPropertyChangedInvocator]
        protected bool ChangeModelProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue)) return false;
            field = newValue;
            this.NotifyPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Sets <see cref="PropertyChanged"/> to null.
        /// </summary>
        protected void ClearPropertyChangedInvocationList() => this.PropertyChanged = null;
    }
}
