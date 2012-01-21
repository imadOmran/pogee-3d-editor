using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using ApplicationCore.ViewModels.Editors;

namespace RoamAreasPlugin
{
    public interface IRoamAreasViewModel : IEditorViewModel
    {
        RoamAreasDataService RoamAreasDataService { get; }
        EQEmu.RoamAreas.RoamArea SelectedArea { get; set; }
        ObservableCollection<EQEmu.RoamAreas.RoamArea> Areas { get; }

        float DefaultMaxZ { get; set; }
        float DefaultMinZ { get; set; }

        EQEmu.RoamAreas.RoamArea CreateNewArea();

        string Zone { get; set; }
    }
}
