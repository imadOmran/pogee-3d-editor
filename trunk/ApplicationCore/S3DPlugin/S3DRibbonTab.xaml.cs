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

using ApplicationCore;
using ApplicationCore.UserControls.Ribbon.Tabs;
using ApplicationCore.ViewModels.Editors;

namespace S3DPlugin
{
    /// <summary>
    /// Interaction logic for PathTab.xaml
    /// </summary>
    /// 
    [AutoRegister]
    public partial class S3DRibbonTab : RibbonTab, IRibbonTab, IS3DControl
    {
        public S3DRibbonTab(S3DRibbonTabViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
        }

        private IS3DViewModel _viewModel = null;
        public IS3DViewModel S3DViewModel
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
                _viewModel = value as IS3DViewModel;
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

        private void TextureUpButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = S3DViewModel as S3DRibbonTabViewModel;
            if (vm != null)
            {
                vm.TextureNumber += 1;
            }
        }

        private void TextureDownButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = S3DViewModel as S3DRibbonTabViewModel;
            if (vm != null && vm.TextureNumber > 0)
            {
                vm.TextureNumber -= 1;
            }
        }

        private void HeadUpButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = S3DViewModel as S3DRibbonTabViewModel;
            if (vm != null)
            {
                vm.HeadNumber += 1;
            }
        }

        private void HeadDownButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = S3DViewModel as S3DRibbonTabViewModel;
            if (vm != null && vm.HeadNumber > 0)
            {
                vm.HeadNumber -= 1;
            }
        }
    }
}
