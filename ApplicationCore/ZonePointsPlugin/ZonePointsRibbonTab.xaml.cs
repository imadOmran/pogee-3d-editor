using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

using Microsoft.Practices.Unity;
using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Win32;

using ApplicationCore.UserControls.Ribbon.Tabs;
using ApplicationCore;
using ApplicationCore.ViewModels.Editors;

namespace ZonePointsPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    [AutoRegister]
    public partial class ZonePointsRibbonTab : RibbonTab, IRibbonTab, IZonePointsControl
    {
        public ZonePointsRibbonTab(ZonePointsRibbonTabViewModel vm)
        {
            DataContext = _viewModel = vm;
            InitializeComponent();

            vm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(vm_PropertyChanged);
        }

        void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedZonePoint":
                    OnObjectSelectionChanged(ZonePointsViewModel.SelectedZonePoint);
                    break;
                default:
                    break;
            }
        }

        private IZonePointsViewModel _viewModel = null;
        public IZonePointsViewModel ZonePointsViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                _viewModel = value;
            }
        }

        public IEditorViewModel ViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                _viewModel = value as IZonePointsViewModel;
            }
        }

        [OptionalDependency]
        public ProjectionCamera Camera3D
        {
            get;
            set;
        }

        [OptionalDependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }


        public event ApplicationCore.UserControls.ObjectSelected ObjectSelected;
        private void OnObjectSelectionChanged(object obj)
        {
            var e = ObjectSelected;
            if (e != null)
            {
                e(this, new ApplicationCore.UserControls.ObjectSelectedEventArgs(obj));
            }
        }

        private void ViewQueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (ZonePointsViewModel != null && 
                ZonePointsViewModel.ZonePointsService != null && 
                ZonePointsViewModel.ZonePointsService.ZonePoints != null)
            {
                var window = new TextWindow(ZonePointsViewModel.ZonePointsService.ZonePoints.GetSQL());
                window.ShowDialog();                
            }
        }

        private EQEmu.Zone.ZonePoints GetZonePoints()
        {
            if (ZonePointsViewModel != null
                && ZonePointsViewModel.ZonePointsService != null)
            {
                return ZonePointsViewModel.ZonePointsService.ZonePoints;
            }
            else return null;
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            var zps = GetZonePoints();
            if ( zps != null )
            {
                var sd = new SaveFileDialog();
                sd.Filter = "XML Files | *.xml";
                if( (bool)sd.ShowDialog() ){
                    ZonePointsViewModel.ZonePointsService.ZonePoints.SaveXML(System.IO.Path.GetDirectoryName(sd.FileName));
                }
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ZonePointsViewModel.ZonePointsService != null)
            {
                var od = new OpenFileDialog();
                od.Filter = "XML Files | *.xml";
                if ((bool)od.ShowDialog())
                {
                    ZonePointsViewModel.ZonePointsService.Zone = "";
                    var zps = GetZonePoints();
                    if (zps != null) zps.LoadXML(od.FileName);
                }                
            }
        }
    }
}
