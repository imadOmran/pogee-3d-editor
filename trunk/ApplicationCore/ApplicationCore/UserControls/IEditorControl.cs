using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationCore.UserControls
{
    public delegate void ObjectSelected(object sender, ObjectSelectedEventArgs args);

    public class ObjectSelectedEventArgs : EventArgs
    {
        public object Object { get; private set; }

        public ObjectSelectedEventArgs(object obj)
        {
            Object = obj;
        }
    }

    public interface IEditorControl
    {        
        ViewModels.Editors.IEditorViewModel ViewModel { get; set; }
        event ObjectSelected ObjectSelected;
    }
}
