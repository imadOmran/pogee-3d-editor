using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using ApplicationCore.ViewModels.Editors;

namespace LineOfSightAreaPlugin
{
    public interface ILineOfSightAreasViewModel : IEditorViewModel
    {
        LineOfSightAreaDataService LineOfSightAreasService { get; }
        EQEmu.LineOfSightAreas.LineOfSightArea SelectedArea { get; set; }
        ObservableCollection<EQEmu.LineOfSightAreas.LineOfSightArea> Areas { get; }

        void NewArea();
    }
}
