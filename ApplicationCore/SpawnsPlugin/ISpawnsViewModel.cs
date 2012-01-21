using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using ApplicationCore.ViewModels.Editors;

namespace SpawnsPlugin
{
    public interface ISpawnsViewModel : IEditorViewModel
    {
        SpawnDataService SpawnsService { get; }
        string Zone { get; set; }
        EQEmu.Spawns.Spawn2 SelectedSpawn { get; set; }

        void CreateNewSpawn(Point3D p);
    }
}
