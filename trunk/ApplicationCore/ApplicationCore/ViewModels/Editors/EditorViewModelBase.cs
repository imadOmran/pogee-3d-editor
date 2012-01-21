using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationCore.ViewModels.Editors
{
    public abstract class EditorViewModelBase : ViewModelBase, IEditorViewModel
    {
        #region IEditorViewModel Members

        abstract public void User3DClickAt( object sender, World3DClickEventArgs e );

        #endregion


        abstract public DataServices.IDataService Service { get; }
    }
}
