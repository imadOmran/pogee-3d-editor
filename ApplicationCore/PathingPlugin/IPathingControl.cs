using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.UserControls;

namespace PathingPlugin
{
    interface IPathingControl : IEditorControl
    {
        IPathingViewModel PathingViewModel { get; set; }
    }
}
