using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Input;

using Microsoft.Practices.Unity;

using ApplicationCore.ViewModels.Editors;

using EQEmu.Grids;

namespace GridsPlugin
{
    public class GridsRibbonTabViewModel : GridsViewModelBase
    {
        public GridsRibbonTabViewModel([Dependency("GridsDataService")] GridsDataService service)
            : base(service)
        {

        }

        [OptionalDependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }

        public override void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            if (e.ActiveRibbonControl as IGridsControl == null) return;

            EQEmu.Grids.Waypoint waypoint = null;

            if (this.GridsService != null && SelectedGrid != null)
            {
                Point3D p = new Point3D(e.PointInWorld.X, e.PointInWorld.Y, e.PointInWorld.Z);
                if (Transform3D != null)
                {
                    Transform3D.TryTransform(p, out p);
                }

                //add a new waypoint
                if (Keyboard.IsKeyDown(Key.LeftShift) && SelectedGrid != null)
                {
                    waypoint = SelectedGrid.GetNewWaypoint();

                    waypoint.X = p.X; waypoint.Y = p.Y; waypoint.Z = p.Z + ZAdjustment;
                    SelectedGrid.AddWaypoint(waypoint);

                    //TODO 3d displayer needs to handle this, hack to update display                    
                    SelectedGrid = SelectedGrid;
                    SelectedWaypoint = waypoint;

                    return;                    
                }                
                //adjust waypoint heading
                if (Keyboard.IsKeyDown(Key.LeftCtrl) && SelectedWaypoint != null)
                {
                    SelectedWaypoint.LookAt(p);

                    //TODO 3d displayer needs to handle this, hack to update display
                    waypoint = SelectedWaypoint;
                    SelectedGrid = SelectedGrid;
                    SelectedWaypoint = waypoint;

                    return;
                }

                if (Keyboard.IsKeyDown(Key.LeftAlt) && SelectedWaypoint != null)
                {
                    SelectedWaypoint.X = p.X; SelectedWaypoint.Y = p.Y; SelectedWaypoint.Z = p.Z + ZAdjustment;

                    //TODO more update hacks
                    waypoint = SelectedWaypoint;
                    SelectedGrid = SelectedGrid;
                    SelectedWaypoint = waypoint;
                }

                List<Waypoint> selectedWaypoints = new List<Waypoint>();
                foreach (var wp in SelectedGrid.Waypoints.Where(
                    x =>
                    {
                        var checkPt = new Point3D(x.X, x.Y, x.Z);
                        Transform3D.TryTransform(checkPt, out checkPt);
                        double dist = 5.0;
                        return e.CheckSelection(checkPt, dist);
                    }))
                {
                    selectedWaypoints.Add(wp);
                }

                if (selectedWaypoints.Count > 0)
                {
                    SelectedWaypoint = selectedWaypoints.ElementAt(0);
                    SelectedWaypoints = selectedWaypoints;
                }
                else
                {
                    waypoint = SelectedGrid.GetNearestWaypoint(p);
                    if (waypoint != null)
                    {
                        SelectedWaypoint = waypoint;
                        SelectedWaypoints = null;
                    }
                }
            }
            
        }
    }
}
