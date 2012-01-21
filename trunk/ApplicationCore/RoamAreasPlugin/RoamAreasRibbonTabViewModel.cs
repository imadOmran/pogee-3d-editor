using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Input;

using Microsoft.Practices.Unity;

using ApplicationCore.ViewModels.Editors;
using SpawnsPlugin;

namespace RoamAreasPlugin
{
    public class RoamAreasRibbonTabViewModel : RoamAreasViewModelBase
    {
        public RoamAreasRibbonTabViewModel([Dependency("RoamAreasDataService")] RoamAreasDataService service) 
            : base(service)
        {

        }

        public override void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            if (e.ActiveRibbonControl as IRoamAreaEditorControl != null)
            {
                Point3D p = new Point3D(e.PointInWorld.X, e.PointInWorld.Y, 0);
                if (Transform3D != null)
                {
                    Transform3D.TryTransform(p, out p);
                }

                if (Keyboard.IsKeyDown(Key.LeftShift) && SelectedArea != null)
                {
                    this.SelectedArea.AddEntry(p);
                    return;
                }
                else if (Keyboard.IsKeyDown(Key.LeftAlt) && SelectedArea != null && RoamAreasDataService.SelectedVertex != null)
                {
                    RoamAreasDataService.SelectedVertex.X = p.X;
                    RoamAreasDataService.SelectedVertex.Y = p.Y;

                    //hack to update display
                    RoamAreasDataService.SelectedArea = SelectedArea;
                }
                else
                {
                    if (SelectedArea != null)
                    {
                        var select = SelectedArea.GetClosestVertex(p);
                        if (select != null)
                        {
                            RoamAreasDataService.SelectedVertex = select;
                        }
                    }
                }
            }
            else if (e.ActiveRibbonControl as ISpawnsControl != null && SpawnDataservice != null)
            {
                if (SpawnDataservice.SelectedSpawn != null)
                {
                    if (RoamAreasDataService.ZoneAreas != null)
                    {
                        var area = RoamAreasDataService.ZoneAreas.RoamAreas.FirstOrDefault(x => x.Id == SpawnDataservice.SelectedSpawn.RoamAreaId);
                        if (area != null)
                        {
                            RoamAreasDataService.SelectedArea = area;
                        }
                    }
                }
            }
        }

        private SpawnDataService _spawnsService;
        [OptionalDependency("SpawnDataService")]
        public SpawnDataService SpawnDataservice
        {
            get { return _spawnsService; }
            set
            {
                _spawnsService = value;
            }
        }

        [OptionalDependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }
    }
}
