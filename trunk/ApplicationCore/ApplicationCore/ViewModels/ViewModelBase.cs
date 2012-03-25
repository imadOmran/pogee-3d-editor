using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ApplicationCore.ViewModels
{
    abstract public class ViewModelBase : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string info)
        {
            var e = PropertyChanged;
            if (e != null)
            {
                e(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
