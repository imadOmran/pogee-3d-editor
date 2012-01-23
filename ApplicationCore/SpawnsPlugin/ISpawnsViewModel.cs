using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using ApplicationCore.ViewModels.Editors;

using EQEmu.Spawns;

namespace SpawnsPlugin
{
    public interface ISpawnsViewModel : IEditorViewModel
    {
        SpawnDataService SpawnsService { get; }
        string Zone { get; set; }
        EQEmu.Spawns.Spawn2 SelectedSpawn { get; set; }
        IEnumerable<Spawn2> SelectedSpawns { get; set; }

        void CreateNewSpawn(Point3D p);
    }
}
