using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationCore.UserControls
{
    public interface IEditorControl
    {        
        ViewModels.Editors.IEditorViewModel ViewModel { get; set; }
    }
}
