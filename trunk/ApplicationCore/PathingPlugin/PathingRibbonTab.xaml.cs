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

using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Practices.Unity;

using ApplicationCore;
using ApplicationCore.UserControls.Ribbon.Tabs;
using ApplicationCore.ViewModels.Editors;

namespace PathingPlugin
{
    /// <summary>
    /// Interaction logic for PathTab.xaml
    /// </summary>
    [AutoRegister]
    public partial class PathingRibbonTab : RibbonTab, IRibbonTab, IPathingControl
    {
        public PathingRibbonTab(PathingRibbonTabViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
            vm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(vm_PropertyChanged);
        }

        void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedNode":
                    OnObjectSelectionChanged(PathingViewModel.SelectedNode);
                    break;
                default:
                    break;
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

        private IPathingViewModel _viewModel = null;
        public IPathingViewModel PathingViewModel
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
                _viewModel = value as IPathingViewModel;
            }
        }
        
        private void MoveToButton_Click(object sender, RoutedEventArgs e)
        {
            EQEmu.Path.Node node = PathingViewModel.SelectedNode;
            Point3D pt;
            if (node != null)
            {
                pt = new Point3D(node.X, node.Y, node.Z);
                if (Transform3D != null)
                {
                    Transform3D.TryTransform(pt, out pt);
                }
            }
            else return;

            if (Camera3D != null)
            {
                Camera3D.Position = pt;
            }
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

        private void RibbonGallery_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = e.NewValue as EQEmu.Path.Node;
            if (node != null)
            {
                PathingViewModel.SelectedNode = node;
            }
        }
    }
}
