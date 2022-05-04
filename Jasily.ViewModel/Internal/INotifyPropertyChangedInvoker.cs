using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Jasily.ViewModel.Internal
{
    internal interface INotifyPropertyChangedInvoker
    {
        public void InvokePropertyChanged(object sender, PropertyChangedEventArgs e);
    }
}
