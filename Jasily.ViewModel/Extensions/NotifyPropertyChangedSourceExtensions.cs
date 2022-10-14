using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;

using Jasily.ViewModel.Internal;

namespace Jasily.ViewModel.Extensions
{
    public static class NotifyPropertyChangedSourceExtensions
    {
        /// <summary>
        /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> with all properties 
        /// that has attribute <see cref="ModelPropertyAttribute"/> 
        /// in this instance.
        /// </summary>
        public static void RefreshProperties(this INotifyPropertyChangedSource source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var handler = source.PropertyChanged;
            if (handler is null)
                return;

            var mapper = RefreshPropertiesMapper.FromType(source.GetType());

            foreach (var args in mapper.GetProperties())
            {
                handler(source, args);
            }
        }

        /// <summary>
        /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> with all properties 
        /// that has specials group attribute <see cref="ModelPropertyAttribute"/>
        /// in this instance.
        /// </summary>
        public static void RefreshProperties(this INotifyPropertyChangedSource source, int group)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var handler = source.PropertyChanged;
            if (handler is null)
                return;

            var mapper = RefreshPropertiesMapper.FromType(source.GetType());

            foreach (var args in mapper.GetPropertiesByGroup(group))
            {
                handler(source, args);
            }
        }
    }
}
