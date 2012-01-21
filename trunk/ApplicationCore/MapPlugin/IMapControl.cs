using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.UserControls;

namespace MapPlugin
{
    public interface IMapControl : IEditorControl
    {
        IMapViewModel MapViewModel { get; set; }
    }
}
