using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
        private PropertyChangedNotificationBlocker _blocker;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentNullException">throw when <paramref name="propertyName"/> is null.</exception>
        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, CallerMemberName] string propertyName = "")
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            
            if (this._blocker != null)
            {
                this._blocker.Add(propertyName);
            }
            else
            {
                this.PropertyChanged?.Invoke(this, propertyName);
            }
            
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

            if (this._blocker != null)
            {
                this._blocker.Add(propertyNames);
            }
            else
            {
                this.PropertyChanged?.Invoke(this, propertyNames);
            }
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

            if (this._blocker != null)
            {
                this._blocker.Add(propertyNames);
            }
            else
            {
                this.PropertyChanged?.Invoke(this, propertyNames);
            }
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

            if (this._blocker != null)
            {
                this._blocker.Add(eventArgs);
            }
            else
            {
                this.PropertyChanged?.Invoke(this, eventArgs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <exception cref="ArgumentNullException">throw when <paramref name="eventArgs"/> is null.</exception>
        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, ItemNotNull] IEnumerable<PropertyChangedEventArgs> eventArgs)
        {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));

            if (this._blocker != null)
            {
                this._blocker.Add(eventArgs);
            }
            else
            {
                this.PropertyChanged?.Invoke(this, eventArgs);
            }
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

        /// <summary>
        /// Block <see cref="PropertyChanged"/> event until <see cref="IDisposable.Dispose"/>.
        /// </summary>
        /// <returns></returns>
        public IDisposable BlockNotifyPropertyChanged()
        {
            if (this._blocker != null)
                throw new InvalidOperationException();

            return new PropertyChangedNotificationBlocker(this);
        }

        private class PropertyChangedNotificationBlocker : IDisposable
        {
            private readonly NotifyPropertyChangedObject _notifyPropertyChangedObject;
            public readonly List<PropertyChangedEventArgs> _eventArgsList = new List<PropertyChangedEventArgs>();

            public PropertyChangedNotificationBlocker(NotifyPropertyChangedObject notifyPropertyChangedObject)
            {
                this._notifyPropertyChangedObject = notifyPropertyChangedObject;
                this._notifyPropertyChangedObject._blocker = this;
            }


            public void Add(string propertyName)
            {
                Debug.Assert(propertyName != null);
                this._eventArgsList.Add(new PropertyChangedEventArgs(propertyName));
            }

            public void Add(IEnumerable<string> propertyNames)
            {
                Debug.Assert(propertyNames != null);
                this._eventArgsList.AddRange(propertyNames.Select(z => new PropertyChangedEventArgs(z)));
            }

            public void Add(PropertyChangedEventArgs eventArgs)
            {
                Debug.Assert(eventArgs != null);
                this._eventArgsList.Add(eventArgs);
            }

            public void Add(IEnumerable<PropertyChangedEventArgs> eventArgs)
            {
                Debug.Assert(eventArgs != null);
                this._eventArgsList.AddRange(eventArgs);
            }

            public void Dispose()
            {
                this._notifyPropertyChangedObject._blocker = null;
                if (this._eventArgsList.Count > 0)
                {
                    this._notifyPropertyChangedObject.NotifyPropertyChanged(this._eventArgsList);
                }
            }
        }
    }
}
