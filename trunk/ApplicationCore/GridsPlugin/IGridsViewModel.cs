using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels.Editors;

namespace GridsPlugin
{
    public interface IGridsViewModel : IEditorViewModel
    {
        GridsDataService GridsService { get; }
        EQEmu.Grids.Grid SelectedGrid { get; set; }
        EQEmu.Grids.Waypoint SelectedWaypoint { get; set; }
        IEnumerable<EQEmu.Grids.Waypoint> SelectedWaypoints { get; set; }

        string Zone { get; set; }
    }
}
