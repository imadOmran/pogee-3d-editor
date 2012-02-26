using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels.Editors;

namespace ZonePointsPlugin
{
    public interface IZonePointsViewModel : IEditorViewModel
    {
        ZonePointsDataService ZonePointsService { get; }
        ICollection<EQEmu.Zone.ZonePoint> ZonePoints { get; }
        EQEmu.Zone.ZonePoint SelectedZonePoint { get; set; }
        IEnumerable<EQEmu.Zone.ZonePoint> SelectedZonePoints { get; set; }
        string Zone { get; set; }
    }
}
