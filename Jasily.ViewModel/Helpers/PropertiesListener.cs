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

        public PropertiesListener(IEnumerable<string> propertyNames)
        {
            _listeners = propertyNames.Select(x => new PropertyListener(x, this)).ToArray();

            if (this._listeners.Length == 0)
                throw new ArgumentException("properties is empty");
        }

        public void Subscribe(object obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            Debug.Assert(this._listeners.Length > 0);

            var listeners = _listeners;

            if (listeners[0].IsSubscribed)
                throw new InvalidOperationException("cannot subscribe again.");

            for (var i = 0; i < listeners.Length; i++)
            {
                listeners[i].Subscribe(i == 0 ? obj : listeners[i - 1].GetPropertyValue());
            }
        }

        public void Unsubscribe()
        {
            Debug.Assert(this._listeners.Length > 0);

            foreach (var listener in _listeners.Reverse())
            {
                listener.Unsubscribe();
            }
        }

        void IInternalPropertiesListener.OnPropertyChanged(PropertyListener listener)
        {
            Debug.Assert(listener != null);
            Debug.Assert(this._listeners.Length > 0);

            var index = Array.IndexOf(_listeners, listener);
            Debug.Assert(index >= 0);

            // unsubscribe changed
            var listeners = _listeners;
            for (var i = listeners.Length - 1; i > index; i--)
            {
                listeners[i].Unsubscribe();
            }

            // resubscribe
            for (var i = index + 1; i < listeners.Length; i++)
            {
                listeners[i].Subscribe(listeners[i - 1].GetPropertyValue());
            }

            this.OnPropertyChanged?.Invoke(this, listeners[^1].GetPropertyValue());
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

        class PropertyListener : IDisposable
        {
            private readonly string _propertyName;
            private readonly IInternalPropertiesListener _propertiesListener;
            private WeakReference<object> _wr;

            public PropertyListener(string propertyName, IInternalPropertiesListener propertiesListener)
            {
                if (propertyName is null) throw new ArgumentNullException(nameof(propertyName));

                this._propertyName = propertyName;
                this._propertiesListener = propertiesListener;
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e?.PropertyName != _propertyName)
                    return;

                this._propertiesListener.OnPropertyChanged(this);
            }

            /// <summary>
            /// Get property value from subscribed object.
            /// </summary>
            /// <returns></returns>
            public object GetPropertyValue()
            {
                if (_wr?.TryGetTarget(out var npc) == true)
                {
                    return GetPropertyValueHelper.GetPropertyValue(npc, _propertyName);
                }

                return null;
            }

            public bool IsSubscribed => _wr != default;

            public void Subscribe(object obj)
            {
                Debug.Assert(_wr is null);

                if (obj is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged += this.OnPropertyChanged;
                }

                _wr = new WeakReference<object>(obj);
            }

            public void Unsubscribe()
            {
                if (_wr?.TryGetTarget(out var o) == true && o is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= this.OnPropertyChanged;
                }

                _wr = default;
            }

            public void Dispose()
            {
                this.Unsubscribe();
                _wr = null;
            }
        }
    }
}
