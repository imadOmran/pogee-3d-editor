using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationCore.UserControls
{
    public interface ITabEditorControl : IEditorControl
    {
        string TabTitle { get; }
    }
}
