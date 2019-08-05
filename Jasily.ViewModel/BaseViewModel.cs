using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Jasily.ViewModel.Internal;
using JetBrains.Annotations;

namespace Jasily.ViewModel
{
    public class BaseViewModel : NotifyPropertyChangedObject
    {
        /// <summary>
        /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/> with all properties that has attribute <see cref="ModelPropertyAttribute"/>
        /// </summary>
        public void RefreshProperties()
        {
            this.NotifyPropertyChanged(RefreshPropertiesMapperProvider.FromType(this.GetType()).GetProperties());
        }

        [NotifyPropertyChangedInvocator]
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue)) return false;
            field = newValue;
            this.NotifyPropertyChanged(propertyName);
            return true;
        }
    }
}
