using System;
using System.ComponentModel;

namespace Jasily.ViewModel
{
    /// <summary>
    /// Indicates that the property should be raises by <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// when you call <see cref="BaseViewModel.RefreshProperties"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ModelPropertyAttribute : Attribute
    {
        public int Group { get; set; }

        /// <summary>
        /// order by asc
        /// </summary>
        public int Order { get; set; }
    }
}
