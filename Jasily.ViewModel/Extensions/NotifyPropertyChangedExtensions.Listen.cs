using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Jasily.ViewModel.Helpers;

namespace Jasily.ViewModel.Extensions
{
    public partial class NotifyPropertyChangedExtensions
    {
        /// <summary>
        /// Listen a property changed event to invoke the <paramref name="callback"/>.
        /// Unlike Binding, this is for programmatic.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="propertySelector"></param>
        /// <param name="callback"></param>
        /// <returns>a <see cref="IDisposable"/> to cancel the listen.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>
        ///   The <paramref name="callback"/> will invoked every time the property changed, not only the property value changed.
        ///   For example, from null to null is possible.
        /// </remarks>
        public static IDisposable ListenPropertyChanged<TModel>(this TModel model,
            Expression<Func<TModel, object>> propertySelector, Action<object> callback)
            where TModel : class, INotifyPropertyChanged
        {
            if (model is null) throw new ArgumentNullException(nameof(model));
            if (propertySelector is null) throw new ArgumentNullException(nameof(propertySelector));
            if (callback is null) throw new ArgumentNullException(nameof(callback));

            var propNames = PropertySelectorHelper.GetPropertyNames(propertySelector).ToArray();
            if (propNames.Length == 0)
                throw new ArgumentException("");

            var listener = new PropertiesListener<TModel>(propNames);
            listener.OnPropertyChanged += v => callback(v);
            listener.Subscribe(model);
            return listener;
        }

        interface IInternalPropertiesListener
        {
            void OnPropertyChanged(PropertyListener listener);
        }

        class PropertiesListener<TModel> : IInternalPropertiesListener, IDisposable
            where TModel : class, INotifyPropertyChanged
        {
            private readonly PropertyListener[] _listeners;

            public PropertiesListener(IEnumerable<string> propertyNames)
            {
                _listeners = propertyNames.Select(x => new PropertyListener(x, this)).ToArray();
            }

            public void Subscribe(TModel obj)
            {
                var listeners = _listeners;
                if (listeners.Length > 0)
                {
                    listeners[0].Subscribe(obj);
                    for (var i = 1; i < listeners.Length; i++)
                    {
                        listeners[i].Subscribe(listeners[i - 1].GetPropertyValue());
                    }
                }
            }

            void IInternalPropertiesListener.OnPropertyChanged(PropertyListener listener)
            {
                Debug.Assert(listener != null);

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

                this.OnPropertyChanged?.Invoke(listeners[^1].GetPropertyValue());
            }

            public void Dispose()
            {
                OnPropertyChanged = null;
                foreach (var listener in _listeners)
                {
                    listener.Dispose();
                }
            }

            public event Action<object> OnPropertyChanged;
        }

        class PropertyListener : IDisposable
        {
            private readonly string _propertyName;
            private readonly IInternalPropertiesListener _propertiesListener;
            private WeakReference<object> _wr;

            public PropertyListener(string propertyName, IInternalPropertiesListener propertiesListener)
            {
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

            public void Subscribe(object obj)
            {
                if (obj is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged += this.OnPropertyChanged;
                }

                if (_wr is null)
                {
                    _wr = new WeakReference<object>(obj);
                }
                else
                {
                    _wr.SetTarget(obj);
                }
            }

            public void Unsubscribe()
            {
                if (_wr?.TryGetTarget(out var o) == true && o is INotifyPropertyChanged npc)
                {
                    npc.PropertyChanged -= this.OnPropertyChanged;
                }

                _wr?.SetTarget(null);
            }

            public void Dispose()
            {
                this.Unsubscribe();
                _wr = null;
            }
        }
    }
}
