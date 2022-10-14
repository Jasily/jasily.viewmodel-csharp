using System;
using System.ComponentModel;

using Jasily.ViewModel.Extensions;

namespace Jasily.ViewModel
{
    /// <summary>
    /// Indicates that the property should be raises by <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// when you calling <see cref="NotifyPropertyChangedSourceExtensions.RefreshProperties"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ModelPropertyAttribute : Attribute
    {
        public int Group { get; set; }

        /// <summary>
        /// order by asc
        /// </summary>
        public int Order { get; set; }
    }
}
