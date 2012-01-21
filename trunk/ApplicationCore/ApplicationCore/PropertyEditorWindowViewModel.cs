using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationCore
{
    public class PropertyEditorWindowViewModel : ViewModels.ViewModelBase
    {
        private object _item = null;
        public object Item
        {
            get { return _item; }
            set
            {
                _item = value;
                NotifyPropertyChanged("Item");
            }
        }
    }
}
