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

namespace LineOfSightAreaPlugin
{
    /// <summary>
    /// Interaction logic for PathTab.xaml
    /// </summary>
    [AutoRegister]
    public partial class LineOfSightAreasRibbonTab : RibbonTab, IRibbonTab, ILineOfSightAreaEditorControl
    {
        public LineOfSightAreasRibbonTab(LineOfSightAreasRibbonTabViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
            vm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(vm_PropertyChanged);
        }

        void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedArea":
                    OnObjectSelectionChanged(LineOfSightAreasViewModel.SelectedArea);
                    break;
                default:
                    break;
            }
        }

        private ILineOfSightAreasViewModel _viewModel = null;
        public ILineOfSightAreasViewModel LineOfSightAreasViewModel
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
                _viewModel = value as ILineOfSightAreasViewModel;
            }
        }
        
        private void NewAreaButton_Click(object sender, RoutedEventArgs e)
        {
            LineOfSightAreasViewModel.NewArea();
        }

        private void RemoveAreaButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.LineOfSightAreasViewModel.LineOfSightAreasService != null &&
                this.LineOfSightAreasViewModel.LineOfSightAreasService.ZoneAreas != null)
            {
                LineOfSightAreasViewModel.LineOfSightAreasService.ZoneAreas.RemoveArea(LineOfSightAreasViewModel.SelectedArea);
                LineOfSightAreasViewModel.SelectedArea = null;
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
    }
}
