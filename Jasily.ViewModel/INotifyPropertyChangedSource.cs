using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Jasily.ViewModel
{
    public interface INotifyPropertyChangedSource
    {
        void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e);
    }
}
