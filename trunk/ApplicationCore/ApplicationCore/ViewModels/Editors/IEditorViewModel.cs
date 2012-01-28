using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace ApplicationCore.ViewModels.Editors
{
    public class World3DClickEventArgs : EventArgs
    {
        public Point3D PointInWorld { get; set; }
        public UserControls.Ribbon.IRibbonItem ActiveRibbonControl { get; set; }
        public object Parameter { get; set; }
        public IsPointInsideSelectionBox CheckSelection { get; set; }

        public World3DClickEventArgs()
        {

        }
    }

    public delegate bool IsPointInsideSelectionBox(Point3D p,double hitTestDistance);

    public interface IEditorViewModel : IViewModel
    {
        void User3DClickAt(object sender,World3DClickEventArgs e);
        DataServices.IDataService Service { get; }
    }
}
