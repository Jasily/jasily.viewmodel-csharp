using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Jasily.ViewModel.Internal;

namespace Jasily.ViewModel.Helpers
{
    public class PropertiesListener : PropertiesListener.IInternalPropertiesListener, IDisposable
    {
        private readonly PropertyListener[] _listeners;
        private WeakReference<object> _weakref;

        public PropertiesListener(IEnumerable<string> propertyNames)
        {
            _listeners = propertyNames.Select(x => new PropertyListener(x, this)).ToArray();

            if (this._listeners.Length == 0)
                throw new ArgumentException("properties is empty");
        }

        private static IEnumerable<object> GetPropertiesValues(object obj, IEnumerable<string> propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (obj is null)
                {
                    yield return null;
                }
                else
                {
                    yield return obj = GetPropertyValueHelper.GetPropertyValue(obj, propertyName);
                }
            }
        }

        public void Subscribe(object obj)
        {
            if (obj is null) 
                throw new ArgumentNullException(nameof(obj));
            if (_weakref != null)
                throw new InvalidOperationException("cannot subscribe again.");

            _weakref = new WeakReference<object>(obj);

            Debug.Assert(this._listeners.Length > 0);

            foreach (var listener in this._listeners)
            {
                listener.Subscribe(obj);
                obj = GetPropertyValueHelper.GetPropertyValue(obj, listener.PropertyName);
                if (obj is null)
                    break;
            }
        }

        public void Unsubscribe()
        {
            Debug.Assert(this._listeners.Length > 0);

            foreach (var listener in _listeners.Reverse())
            {
                listener.Unsubscribe();
            }

            _weakref = null;
        }

        void IInternalPropertiesListener.OnPropertyChanged(PropertyListener listener)
        {
            Debug.Assert(listener != null);
            Debug.Assert(this._listeners.Length > 0);
            Debug.Assert(_weakref != null);

            if (!_weakref.TryGetTarget(out var obj))
            {
                // source
                this.Unsubscribe();
                return;
            }

            var index = Array.IndexOf(_listeners, listener);
            Debug.Assert(index >= 0);

            var listeners = _listeners;

            // resubscribe
            var values = GetPropertiesValues(obj, listeners.Select(x => x.PropertyName)).ToList();
            for (var i = 1; i < listeners.Length; i++)
            {
                listeners[i].Resubscribe(values[i - 1]);
            }

            this.OnPropertyChanged?.Invoke(this, values[^1]);
        }

        public void Dispose()
        {
            OnPropertyChanged = null;
            this.Unsubscribe();
        }

        public event EventHandler<object> OnPropertyChanged;

        interface IInternalPropertiesListener
        {
            void OnPropertyChanged(PropertyListener listener);
        }

        class PropertyListener : Disposable
        {
            public readonly string PropertyName;
            private readonly IInternalPropertiesListener _propertiesListener;
            private WeakReference<object> _weakref;

            public PropertyListener(string propertyName, IInternalPropertiesListener propertiesListener)
            {
                if (propertyName is null) throw new ArgumentNullException(nameof(propertyName));

                this.PropertyName = propertyName;
                this._propertiesListener = propertiesListener;
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e?.PropertyName != PropertyName)
                    return;

                this._propertiesListener.OnPropertyChanged(this);
            }

            public void Resubscribe(object obj)
            {
                if (_weakref?.TryGetTarget(out var o) != true || !ReferenceEquals(o, obj))
                {
                    Unsubscribe();
                    Subscribe(obj);
                }
            }

            public void Subscribe(object obj)
            {
                Debug.Assert(_weakref is null);

                this.ThrowIfDisposed();

                if (obj is null)
                    return;

                if (obj is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged += this.OnPropertyChanged;
                }

                _weakref = new WeakReference<object>(obj);
            }

            public void Unsubscribe()
            {
                if (_weakref?.TryGetTarget(out var o) == true && o is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= this.OnPropertyChanged;
                }

                _weakref = null;
            }

            protected override void DisposeCore() => this.Unsubscribe();
        }
    }
}
