using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

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
        private PropertiesBatchChanges _propertiesBatchChanges;

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

        protected virtual IEqualityComparer<T> GetPropertyValueComparer<T>() => EqualityComparer<T>.Default;

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
            var comparer = this.GetPropertyValueComparer<T>();
            if (comparer?.Equals(field, newValue) ?? false) return false;
            var oldValue = field;
            field = newValue;
            if (this._propertiesBatchChanges != null)
            {
                this._propertiesBatchChanges.AddChange(propertyName, oldValue, newValue, comparer);
            }
            else
            {
                this.NotifyPropertyChanged(propertyName);
            } 
            return true;
        }

        /// <summary>
        /// Sets <see cref="PropertyChanged"/> to null.
        /// </summary>
        protected void ClearPropertyChangedInvocationList() => this.PropertyChanged = null;

        /// <summary>
        /// Block event <see cref="PropertyChanged"/> until called <see cref="IDisposable.Dispose"/>.
        /// 
        /// Note: 
        ///   - this should only run on the UI thread; 
        ///   - this is not thread safety;
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

        /// <summary>
        /// <see cref="BeginBatchChangeModelProperties"/> allow you to to call <see cref="ChangeModelProperty"/> many times 
        /// but only raises event <see cref="PropertyChanged"/> once after call <see cref="IDisposable.Dispose"/>.
        /// 
        /// Also, if a property changed to a new value then change back to the old value, 
        /// the event <see cref="PropertyChanged"/> won't be raises.
        /// 
        /// Note: 
        ///   - this should only run on the UI thread; 
        ///   - this is not thread safety;
        /// </summary>
        /// <returns></returns>
        public IDisposable BeginBatchChangeModelProperties()
        {
            if (this._propertiesBatchChanges != null)
                throw new InvalidOperationException();

            return this._propertiesBatchChanges = new PropertiesBatchChanges(this);
        }

        private class PropertiesBatchChanges : IDisposable 
        {
            private readonly List<string> _orderedPropertyNames = new List<string>();
            private readonly Dictionary<string, (object originValue, bool isChanged)> _propertyValues =
                new Dictionary<string, (object originValue, bool isChanged)>();
            private NotifyPropertyChangedObject _obj;

            public PropertiesBatchChanges(NotifyPropertyChangedObject notifyPropertyChangedObject)
            {
                this._obj = notifyPropertyChangedObject;
            }

            public void AddChange<T>(string propertyName, T oldValue, T newValue, IEqualityComparer<T> comparer)
            {
                var originValue = this._propertyValues.TryGetValue(propertyName, out var entry)
                    ? (T)entry.originValue
                    : oldValue;
                var isChanged = comparer?.Equals(originValue, newValue) != true; // if comparer is null, should be changed.
                this._propertyValues[propertyName] = (originValue, isChanged);
                this._orderedPropertyNames.Add(propertyName);
            }

            public IEnumerable<string> GetChangedPropertyNames()
            {
                return this._orderedPropertyNames
                    .Distinct()
                    .Where(z => this._propertyValues[z].isChanged);
            }

            public void Dispose()
            {
                var obj = this._obj;
                if (obj == null)
                    throw new ObjectDisposedException(this.ToString());
                this._obj = null;

                var self = obj._propertiesBatchChanges;
                if (self != this)
                    throw new InvalidOperationException();
                obj._propertiesBatchChanges = null;

                obj.NotifyPropertyChanged(this.GetChangedPropertyNames());
            }
        }
    }
}
