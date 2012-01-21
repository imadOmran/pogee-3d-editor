using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels.Editors;

namespace PathingPlugin
{
    public interface IPathingViewModel : IEditorViewModel
    {
        PathingDataService PathingService { get; }
        EQEmu.Path.Node SelectedNode { get; set; }
    }
}
