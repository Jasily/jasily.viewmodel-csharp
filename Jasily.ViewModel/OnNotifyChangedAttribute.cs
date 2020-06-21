using System;
using System.ComponentModel;

namespace Jasily.ViewModel
{
    /// <summary>
    /// Indicates that the method should be call before raises <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// when you calling <see cref="BaseViewModel.RefreshProperties"/>.
    /// 
    /// The target method require a parameterless instance method with return void.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnNotifyChangedAttribute : Attribute
    {
        public OnNotifyChangedAttribute(string propertyName)
        {
            this.PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        }

        public string PropertyName { get; }
    }
}
