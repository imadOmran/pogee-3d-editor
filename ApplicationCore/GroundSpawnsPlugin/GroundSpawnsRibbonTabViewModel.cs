using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Input;

using Microsoft.Practices.Unity;

using ApplicationCore.ViewModels.Editors;

namespace GroundSpawnsPlugin
{
    public class GroundSpawnsRibbonTabViewModel : GroundSpawnsViewModelBase
    {
        public GroundSpawnsRibbonTabViewModel([Dependency("GroundSpawnsDataService")] GroundSpawnsDataService service)
            : base(service)
        {

        }

        [OptionalDependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        
        }

        //private double _zAdjustment = 2.0;
        //public double ZAdjustment
        //{
        //    get { return _zAdjustment; }
        //    set
        //    {
        //        _zAdjustment = value;
        //        NotifyPropertyChanged("ZAdjustment");
        //    }
        //}

        public override void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            if (e.ActiveRibbonControl as IGroundSpawnsControl == null) return;
            
            if (this.GroundSpawnsService != null)
            {
                Point3D p = new Point3D(e.PointInWorld.X, e.PointInWorld.Y, e.PointInWorld.Z);
                if (Transform3D != null)
                {
                    Transform3D.TryTransform(p, out p);
                }

                if (Keyboard.IsKeyDown(Key.LeftAlt) && SelectedGroundSpawn != null)
                {
                    SelectedGroundSpawn.CenterPositionAt(new Point3D(p.X,p.Y,p.Z+ZAdjustment) );
                    return;
                }

                if (e.CheckSelection != null)
                {
                    var selections = GroundSpawnsService.ZoneGroundSpawns.GroundSpawns.Where(x =>
                    {
                        var pt3d = new Point3D(x.MidPoint.X, x.MidPoint.Y, x.MaxZ);
                        pt3d = Transform3D.Transform(pt3d);
                        return e.CheckSelection(pt3d,0);
                    });
                    if (selections.Count() > 0)
                    {
                        SelectedGroundSpawn = selections.ElementAt(0);

                        if (selections.Count() > 1)
                        {
                            SelectedGroundSpawns = selections;
                        }
                        else
                        {
                            SelectedGroundSpawns = null;
                        }
                        return;
                    }
                }

                var spawn = GroundSpawnsService.GetClosestGroundSpawn(p);
                if (spawn != null)
                {
                    SelectedGroundSpawn = spawn;
                    SelectedGroundSpawns = null;
                }
            }            
        }
    }
}
