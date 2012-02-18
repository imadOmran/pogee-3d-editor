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

using Microsoft.Windows.Controls.Ribbon;

using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.UserControls.Ribbon.Tabs;
using ApplicationCore.ViewModels.Editors;

namespace RoamAreasPlugin
{
    /// <summary>
    /// Interaction logic for PathTab.xaml
    /// </summary>
    [AutoRegister]
    public partial class RoamAreasRibbonTab : RibbonTab, IRibbonTab, IRoamAreaEditorControl
    {
        public RoamAreasRibbonTab(RoamAreasRibbonTabViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
        }

        private IRoamAreasViewModel _viewModel = null;
        public IRoamAreasViewModel RoamAreaViewModel
        {
            get { return _viewModel; }

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
                _viewModel = value as IRoamAreasViewModel;
            }
        }
        
        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoamAreaViewModel.RoamAreasDataService != null && RoamAreaViewModel.RoamAreasDataService.ZoneAreas != null)
            {
                SaveFileDialog sd = new SaveFileDialog();
                if ((bool)sd.ShowDialog())
                {
                    RoamAreaViewModel.RoamAreasDataService.ZoneAreas.SaveQueryToFile(sd.FileName);            
                }
                return;                
            }
        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (RoamAreaViewModel.RoamAreasDataService != null && RoamAreaViewModel.RoamAreasDataService.ZoneAreas != null)
            {
                RoamAreaViewModel.CreateNewArea();
            }
        }

        private void EditSpawnButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoamAreaViewModel.SelectedArea != null)
            {
                var window = new PropertyEditorWindow(RoamAreaViewModel.SelectedArea);
                window.ShowDialog();
                RoamAreaViewModel.SelectedArea = RoamAreaViewModel.SelectedArea;
            }
        }
    }
}
