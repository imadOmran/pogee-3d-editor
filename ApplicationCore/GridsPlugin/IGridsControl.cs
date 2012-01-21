using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.UserControls;

namespace GridsPlugin
{
    interface IGridsControl : IEditorControl
    {
        IGridsViewModel GridsViewModel { get; set; }
    }
}
