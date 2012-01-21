using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels.Editors;

namespace GroundSpawnsPlugin
{
    public interface IGroundSpawnsViewModel : IEditorViewModel
    {
        GroundSpawnsDataService GroundSpawnsService { get; }
        EQEmu.GroundSpawns.GroundSpawn SelectedGroundSpawn { get; set; }
        IEnumerable<EQEmu.GroundSpawns.GroundSpawn> SelectedGroundSpawns { get; set; }
        string Zone { get; set; }
    }
}
