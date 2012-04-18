using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationCore.UserControls
{
    public class EditorControl : IEditorControl
    {
        public event OnObjectSelected ObjectSelected;
        protected void OnObjectSelected(object o)
        {
            var e = ObjectSelected;
            if(e!=null)
            {
                e(this,new ObjectSelectedEventArgs(o));
            }
        }

        private ViewModels.Editors.IEditorViewModel _viewModel;
        public ViewModels.Editors.IEditorViewModel ViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                _viewModel = value;
            }
        }
    }
}
