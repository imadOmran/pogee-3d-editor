using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Input;

using Microsoft.Practices.Unity;

using ApplicationCore.ViewModels.Editors;
using GridsPlugin;

namespace SpawnsPlugin
{
    public class SpawnsRibbonTabViewModel : SpawnsViewModelBase
    {
        public SpawnsRibbonTabViewModel([Dependency("SpawnDataService")] SpawnDataService service)
            : base(service)
        {
            ZAdjustment = 2.0;
            DefaultSpawnGroup = 0;
            DefaultGrid = 0;
            DefaultRoamArea = 0;
        }

        [Dependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }

        public double ZAdjustment
        {
            get;
            set;
        }

        public int DefaultSpawnGroup
        {
            get;
            set;
        }

        public int DefaultGrid
        {
            get;
            set;
        }

        public int DefaultRoamArea
        {
            get;
            set;
        }

        public override void CreateNewSpawn(Point3D p)
        {
            if (SpawnsService != null && SpawnsService.ZoneSpawns != null)
            {
                var newspawn = SpawnsService.ZoneSpawns.GetNewSpawn();
                newspawn.X = p.X; newspawn.Y = p.Y; newspawn.Z = p.Z + ZAdjustment;

                newspawn.RoamAreaId = DefaultRoamArea;
                newspawn.GridId = DefaultGrid;
                newspawn.SpawnGroupId = DefaultSpawnGroup;

                SpawnsService.ZoneSpawns.AddSpawn(newspawn);
                SelectedSpawn = newspawn;
            }
        }

        public override void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            if (e.ActiveRibbonControl as ISpawnsControl == null) return;
            
            if (SpawnsService != null && SpawnsService.ZoneSpawns != null )
            {
                Point3D p = new Point3D(e.PointInWorld.X, e.PointInWorld.Y, e.PointInWorld.Z);

                if (Transform3D != null)
                {
                    Transform3D.TryTransform(p, out p);
                }

                if (Keyboard.IsKeyDown(Key.LeftAlt) && SelectedSpawn != null)
                {
                    SelectedSpawn.X = p.X; SelectedSpawn.Y = p.Y; SelectedSpawn.Z = p.Z + ZAdjustment;
                    //TODO hack visual update
                    SelectedSpawn = SelectedSpawn;
                    return;
                }

                if(Keyboard.IsKeyDown(Key.LeftCtrl) && SelectedSpawn != null)
                {
                    if (SelectedSpawns != null)
                    {
                        foreach (var s in SelectedSpawns)
                        {
                            s.LookAt(p);
                        }
                    }

                    SelectedSpawn.LookAt(p);
                    //TODO visual update hack
                    SelectedSpawn = SelectedSpawn;
                    SelectedSpawns = SelectedSpawns;
                    return;
                }

                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    if (SpawnsService != null && SpawnsService.ZoneSpawns != null)
                    {
                        CreateNewSpawn(p);
                    }
                    return;
                }

                List<EQEmu.Spawns.Spawn2> selSpawns = new List<EQEmu.Spawns.Spawn2>();
                foreach (var s in SpawnsService.ZoneSpawns.Spawns.Where(
                    x =>
                    {
                        var spt = new Point3D(x.X, x.Y, x.Z);
                        Transform3D.TryTransform(spt, out spt);
                        return e.CheckSelection(spt);
                    }))
                {
                    selSpawns.Add(s);
                }

                if (selSpawns.Count > 0)
                {
                    SelectedSpawn = selSpawns.ElementAt(0);
                    SelectedSpawns = selSpawns;
                }
                else
                {
                    var spawn = SpawnsService.ZoneSpawns.GetNearestSpawn(p);

                    if (spawn != null)
                    {
                        SelectedSpawn = spawn;
                        SelectedSpawns = null;
                    }
                }

                //SpawnsService.World3DMouseClickAt(e.PointInWorld);
            }


            //throw new NotImplementedException();
        }
        
        private GridsDataService _gridsService = null;
        [OptionalDependency("GridsDataService")]
        public GridsDataService GridsService
        {
            get { return _gridsService; }
            set
            {
                _gridsService = value;
                NotifyPropertyChanged("GridsService");
            }
        }

        public override IEnumerable<EQEmu.Spawns.Spawn2> SelectedSpawns
        {
            get
            {
                if (SpawnsService != null)
                {
                    return SpawnsService.SelectedSpawns;
                }
                else return null;
            }
            set
            {
                if (SpawnsService != null)
                {
                    SpawnsService.SelectedSpawns = value;
                    NotifyPropertyChanged("SelectedSpawns");
                }
            }
        }


        public override EQEmu.Spawns.Spawn2 SelectedSpawn
        {
            get
            {
                if (SpawnsService != null)
                {
                    return SpawnsService.SelectedSpawn;
                }
                else return null;
            }
            set
            {
                if (SpawnsService != null)
                {
                    SpawnsService.SelectedSpawn = value;
                    NotifyPropertyChanged("SelectedSpawn");
                }                
            }
        }
    }
}
