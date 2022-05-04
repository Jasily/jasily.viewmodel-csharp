using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            var propertyNames = PropertySelectorHelper.GetPropertyNames(propertySelector).ToArray();
            var listener = new PropertiesListener(propertyNames);
            listener.OnPropertyChanged += v => callback(v);
            listener.Subscribe(model);
            return listener;
        }

        public static IDisposable ListenPropertyChanged<TModel>(this TModel model,
            IEnumerable<string> propertyNames, Action<object> callback)
            where TModel : class, INotifyPropertyChanged
        {
            if (model is null) throw new ArgumentNullException(nameof(model));
            if (propertyNames is null) throw new ArgumentNullException(nameof(propertyNames));
            if (callback is null) throw new ArgumentNullException(nameof(callback));

            var listener = new PropertiesListener(propertyNames);
            listener.OnPropertyChanged += v => callback(v);
            listener.Subscribe(model);
            return listener;
        }
    }
}
