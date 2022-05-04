using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using Jasily.ViewModel.Internal;

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
            var mapper = RefreshPropertiesMapper.FromType(this.GetType());
            this.RefreshProperties(mapper.GetPropertiesChangedData());
        }

        /// <summary>
        /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/> with all properties 
        /// that has specials group attribute <see cref="ModelPropertyAttribute"/>
        /// in this instance.
        /// </summary>
        public void RefreshProperties(int group)
        {
            var mapper = RefreshPropertiesMapper.FromType(this.GetType());
            this.RefreshProperties(mapper.GetPropertiesChangedData(group));
        }

        private void RefreshProperties(IReadOnlyCollection<PropertyChangedEventArgs> eventArgsList)
        {
            Debug.Assert(eventArgsList != null);
            this.NotifyPropertyChanged(eventArgsList);
        }
    }
}
