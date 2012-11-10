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
using System.IO;

using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.UserControls.Ribbon.Tabs;
using ApplicationCore.UserControls;
using ApplicationCore.ViewModels.Editors;

namespace NpcTypePlugin
{
    /// <summary>
    /// 
    /// </summary>
    [AutoRegister]
    public partial class NpcTypeRibbonTab : RibbonTab, IRibbonTab
    {
        private IEditorViewModel _viewModel;

        public NpcTypeRibbonTab(NpcTypeEditViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
        }

        public IEditorViewModel ViewModel
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

        public NpcTypeEditViewModel NpcEditViewModel
        {
            get
            {
                return _viewModel as NpcTypeEditViewModel;
            }
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            var od = new OpenFileDialog();
            od.Filter = "XML Files | *.npctypes.xml";
            if ((bool)od.ShowDialog() == true)
            {
                NpcEditViewModel.OpenXML(od.FileName);
            }
        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            var sd = new SaveFileDialog();
            if ((bool)sd.ShowDialog() == true)
            {
                NpcEditViewModel.SaveXML(System.IO.Path.GetDirectoryName(sd.FileName));
            }
        }

        private void ViewQueryButton_Click(object sender, RoutedEventArgs e)
        {
            var sql = NpcEditViewModel.NpcManager.GetSQL();
            var window = new TextWindow(sql);
            window.ShowDialog();
        }

        private void OpenGlobalModelButton_Click(object sender, RoutedEventArgs e)
        {
            var od = new OpenFileDialog();
            od.Filter = "S3D Files | *_chr.s3d";
            if ((bool)od.ShowDialog())
            {
                var file = od.FileName;
                NpcEditViewModel.OpenModels(NpcTypeEditViewModel.ModelSource.Global, file);
            }
        }

        private void OpenZoneModelButton_Click(object sender, RoutedEventArgs e)
        {
            var od = new OpenFileDialog();
            od.Filter = "S3D Files | *_chr.s3d";
            if ((bool)od.ShowDialog())
            {
                var file = od.FileName;
                NpcEditViewModel.OpenModels(NpcTypeEditViewModel.ModelSource.Zone, file);
            }
        }
    }
}
