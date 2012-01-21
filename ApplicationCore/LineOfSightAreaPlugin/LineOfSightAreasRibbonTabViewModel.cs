using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Input;

using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore.ViewModels.Editors;

namespace LineOfSightAreaPlugin
{
    public class LineOfSightAreasRibbonTabViewModel : LineOfSightAreasViewModelBase
    {
        public LineOfSightAreasRibbonTabViewModel([Dependency("LineOfSightAreaDataService")] LineOfSightAreaDataService service) 
            : base(service)
        {

        }

        public void OpenFile()
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Line of Sight Area Files (.los)|*.los|All Files (*.*)|*.*";
            if ((bool)fd.ShowDialog())
            {
                LineOfSightAreasService.OpenFile(fd.FileName);                
            }
        }

        public void NewFile()
        {
            LineOfSightAreasService.NewFile();
        }

        public void SaveFile()
        {
            SaveFileDialog sd = new SaveFileDialog();
            if ((bool)sd.ShowDialog())
            {
                LineOfSightAreasService.SaveFile(sd.FileName);
            }
        }

        public override bool CanExecuteNewCommand(object arg)
        {
            return (LineOfSightAreasService != null);
        }

        public override void ExecuteNewCommand(object arg)
        {
            this.NewFile();
        }

        public override bool CanExecuteOpenCommand(object arg)
        {
            return (LineOfSightAreasService != null);
        }

        public override void ExecuteOpenCommand(object arg)
        {
            this.OpenFile();
        }

        public override bool CanExecuteSaveCommand(object arg)
        {
            return (LineOfSightAreasService != null && LineOfSightAreasService.ZoneAreas != null);
        }

        public override void ExecuteSaveCommand(object arg)
        {
            this.SaveFile();
        }

        [OptionalDependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }

        public double DefaultMinZ
        {
            get;
            set;
        }

        public double DefaultMaxZ
        {
            get;
            set;
        }

        public override void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            if ( e.ActiveRibbonControl as ILineOfSightAreaEditorControl == null ) return;

            Point3D p = new Point3D( e.PointInWorld.X, e.PointInWorld.Y, 0 );
            if ( Transform3D != null ) {
                Transform3D.TryTransform( p, out p );
            }

            if ( Keyboard.IsKeyDown( Key.LeftShift ) && SelectedArea != null ) {
                this.SelectedArea.AddVertex( p );
                return;
            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt) && SelectedArea != null && LineOfSightAreasService.SelectedVertex != null)
            {
                LineOfSightAreasService.SelectedArea.MoveVertex(LineOfSightAreasService.SelectedVertex, p);
                LineOfSightAreasService.SelectedVertex = p;

                //hack to update display
                LineOfSightAreasService.SelectedArea = SelectedArea;
            }

            if ( SelectedArea != null ) {
                LineOfSightAreasService.SelectedVertex = new Point3D( p.X, p.Y, p.Z );
            }
        }

        public override void NewArea()
        {
            var area = new EQEmu.LineOfSightAreas.LineOfSightArea();
            area.MaxZ = DefaultMaxZ;
            area.MinZ = DefaultMinZ;

            if (LineOfSightAreasService != null && LineOfSightAreasService.ZoneAreas != null)
            {
                LineOfSightAreasService.ZoneAreas.AddArea(area);
            }
        }
    }
}
