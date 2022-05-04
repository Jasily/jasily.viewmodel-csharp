using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Jasily.ViewModel.Extensions;
using Jasily.ViewModel.Internal;

using JetBrains.Annotations;

namespace Jasily.ViewModel
{
    /// <summary>
    /// The base class for interface <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    public class NotifyPropertyChangedObject : INotifyPropertyChanged, INotifyPropertyChangedInvoker
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private INotifyPropertyChangedInvoker _invoker;
        private PropertiesBatchChanges _propertiesBatchChanges;

        public NotifyPropertyChangedObject()
        {
            _invoker = this;
        }

        void INotifyPropertyChangedInvoker.InvokePropertyChanged(object sender, PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(sender, e);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentNullException">throw when <paramref name="propertyName"/> is null.</exception>
        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([NotNull, CallerMemberName] string propertyName = "")
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            this._invoker.InvokePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

            var invoker = this._invoker;
            for (var i = 0; i < propertyNames.Length; i++)
            {
                invoker.InvokePropertyChanged(this, new PropertyChangedEventArgs(propertyNames[i]));
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

            var invoker = this._invoker;
            foreach (var property in propertyNames)
            {
                invoker.InvokePropertyChanged(this, new PropertyChangedEventArgs(property));
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

            var invoker = this._invoker;
            for (var i = 0; i < eventArgs.Length; i++)
            {
                invoker.InvokePropertyChanged(this, eventArgs[i]);
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

            var invoker = this._invoker;
            foreach (var eventArg in eventArgs)
            {
                invoker.InvokePropertyChanged(this, eventArg);
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
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This should run on the UI thread, or user specify a <paramref name="executor"/> to invoke the <see cref="INotifyPropertyChanged.PropertyChanged"/>;
        /// </remarks>
        public IDisposable BlockNotifyPropertyChanged(Action<Action> executor = default)
        {
            PropertyChangedBlocker blocker = null;

            while (true)
            {
                var invoker = this._invoker; // don't need memory barrier

                Debug.Assert(ReferenceEquals(invoker, this) || invoker is PropertyChangedBlocker);

                if ((invoker as PropertyChangedBlocker)?.References.TryGetReference(out var disposable) == true)
                {
                    return disposable;
                }

                if (blocker == null)
                    blocker = new PropertyChangedBlocker(this, executor);

                Interlocked.CompareExchange(ref this._invoker, blocker, this);
            }
        }

        private class PropertyChangedBlocker : Disposable, INotifyPropertyChangedInvoker
        {
            private static readonly Action<Action> s_Executor = a => a.Invoke();
            private readonly NotifyPropertyChangedObject _obj;
            private readonly Action<Action> _executor;
            public readonly BlockingCollection<(object, PropertyChangedEventArgs)> _eventArgsLists = new BlockingCollection<(object, PropertyChangedEventArgs)>();

            public PropertyChangedBlocker(NotifyPropertyChangedObject obj, Action<Action> executor)
            {
                Debug.Assert(obj != null);
                this._obj = obj;
                this._executor = executor;
                this.References = new DisposableReferences(this);
            }

            public DisposableReferences References { get; }

            protected override void DisposeCore()
            {
                if (Interlocked.CompareExchange(ref this._obj._invoker, this._obj, this) != this)
                    throw new InvalidOperationException();

                _eventArgsLists.CompleteAdding();

                var eventArgsList = _eventArgsLists.GetConsumingEnumerable().ToList();

                if (eventArgsList.Count > 0)
                {
                    (_executor ?? s_Executor)(() =>
                    {
                        var invoker = this._obj._invoker;
                        Debug.Assert(invoker != this);
                        foreach (var (sender, e) in eventArgsList)
                        {
                            invoker.InvokePropertyChanged(sender, e);
                        }
                    });
                }
            }

            public void InvokePropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (!this._eventArgsLists.TryAdd((sender, e)))
                {
                    (_executor ?? s_Executor)(() =>
                    {
                        var invoker = this._obj._invoker;
                        Debug.Assert(invoker != this);
                        invoker.InvokePropertyChanged(sender, e);
                    });
                }
            }
        }

        /// <summary>
        /// <see cref="BeginBatchChangeModelProperties"/> allow you to to call <see cref="ChangeModelProperty"/> many times 
        /// but only raises event <see cref="PropertyChanged"/> once after call <see cref="IDisposable.Dispose"/>.
        /// 
        /// Also, if a property changed to a new value then change back to the old value, 
        /// the event <see cref="PropertyChanged"/> won't be raises.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is not thread safety;
        /// This must run on the UI thread;
        /// </remarks>
        public IDisposable BeginBatchChangeModelProperties()
        {
            if (this._propertiesBatchChanges != null)
                throw new InvalidOperationException();

            return this._propertiesBatchChanges = new PropertiesBatchChanges(this);
        }

        private class PropertiesBatchChanges : Disposable
        {
            private readonly List<string> _orderedPropertyNames = new List<string>();
            private readonly Dictionary<string, (object originValue, bool isChanged)> _propertyValues =
                new Dictionary<string, (object originValue, bool isChanged)>();
            private readonly NotifyPropertyChangedObject _obj;

            public PropertiesBatchChanges(NotifyPropertyChangedObject obj)
            {
                Debug.Assert(obj != null);
                this._obj = obj;
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

            protected override void DisposeCore()
            {
                if (this._obj._propertiesBatchChanges != this)
                    throw new InvalidOperationException();
                this._obj._propertiesBatchChanges = null;
                this._obj.NotifyPropertyChanged(this.GetChangedPropertyNames());
            }
        }
    }
}
