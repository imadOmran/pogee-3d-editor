using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Input;

using Microsoft.Practices.Unity;

using ApplicationCore.ViewModels.Editors;

namespace ZonePointsPlugin
{
    public class ZonePointsRibbonTabViewModel : ZonePointsViewModelBase
    {
        public ZonePointsRibbonTabViewModel([Dependency("ZonePointsDataService")] ZonePointsDataService service)
            : base(service)
        {

        }

        [OptionalDependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }

        protected override void OnLeftMouseClick(object sender, World3DClickEventArgs e)
        {
            base.OnLeftMouseClick(sender, e);
            var pt = e.PointInWorld;
            Transform3D.TryTransform(pt, out pt);

            if (Keyboard.IsKeyDown(Key.LeftAlt) && SelectedZonePoint != null)
            {
                SelectedZonePoint.X = (float)pt.X;
                SelectedZonePoint.Y = (float)pt.Y;
                SelectedZonePoint.Z = (float)(pt.Z + ZAdjustment);
                return;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && SelectedZonePoint != null)
            {
                SelectedZonePoint.TargetX = (float)pt.X;
                SelectedZonePoint.TargetY = (float)pt.Y;
                SelectedZonePoint.TargetZ = (float)(pt.Z + ZAdjustment);
                return;
            }

            if (ZonePointsService != null && ZonePointsService.ZonePoints != null)
            {
                var zp = ZonePointsService.ZonePoints.GetClosestPoint(pt, true);
                if (zp != null)
                {
                    SelectedZonePoint = zp;
                }
            }
        }

        public override void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            if (e.ActiveRibbonControl as IZonePointsControl == null) return;
            base.User3DClickAt(sender, e);
        }
    }
}
