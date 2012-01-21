using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels.Editors;

namespace MapPlugin
{
    public interface IMapViewModel : IEditorViewModel
    {
        MapDataService MapService { get; }
    }
}
