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
using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.UserControls.Ribbon.Tabs;
using ApplicationCore.ViewModels.Editors;

namespace SpawnExtractorPlugin
{
    [AutoRegister]
    public partial class SpawnExtractorRibbonTab : RibbonTab, IRibbonTab, ISpawnExtractorControl
    {
        public SpawnExtractorRibbonTab(SpawnExtractorTabViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
        }

        private SpawnExtractorTabViewModel _viewModel = null;
        public SpawnExtractorTabViewModel ExtractorViewModel
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
                _viewModel = value as SpawnExtractorTabViewModel;
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

        private void NPCQueryButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new TextWindow(ExtractorViewModel.NPCQuery());
            window.ShowDialog();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var od = new SaveFileDialog();
            if ((bool)od.ShowDialog())
            {
                var dir = System.IO.Path.GetDirectoryName(od.FileName);
                ExtractorViewModel.SaveXML(dir);
            }
        }
    }
}
