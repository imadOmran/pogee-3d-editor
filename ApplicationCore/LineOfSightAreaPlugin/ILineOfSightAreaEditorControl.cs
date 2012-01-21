using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.UserControls;

namespace LineOfSightAreaPlugin
{
    public interface ILineOfSightAreaEditorControl : IEditorControl
    {
        ILineOfSightAreasViewModel LineOfSightAreasViewModel { get; set; }
    }
}
