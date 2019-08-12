using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
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
            this.NotifyPropertyChanged(RefreshPropertiesMapper.FromType(this.GetType()).GetProperties());
        }
    }
}
