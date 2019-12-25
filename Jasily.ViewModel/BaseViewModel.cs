using System;
using System.Buffers;
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
        /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/> with all properties 
        /// that has attribute <see cref="ModelPropertyAttribute"/> 
        /// in this instance.
        /// </summary>
        public void RefreshProperties()
        { 
            this.NotifyPropertyChanged(RefreshPropertiesMapper.FromType(this.GetType()).Properties);
        }

        /// <summary>
        /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/> with all properties 
        /// that has specials group attribute <see cref="ModelPropertyAttribute"/>
        /// in this instance.
        /// </summary>
        public void RefreshProperties(int group)
        {
            this.NotifyPropertyChanged(RefreshPropertiesMapper.FromType(this.GetType()).GetProperties(group));
        }
    }
}
