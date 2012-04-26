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

namespace DoorsPlugin
{
    /// <summary>
    /// Interaction logic for PathTab.xaml
    /// </summary>
    /// 
    [AutoRegister]
    public partial class DoorsRibbonTab : RibbonTab, IRibbonTab, IDoorsControl
    {
        public DoorsRibbonTab(DoorsRibbonTabViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
            _viewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_viewModel_PropertyChanged);
        }

        void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedDoor":
                    OnObjectSelectionChanged(_viewModel.SelectedDoor);
                    break;
            }
        }

        private IDoorsViewModel _viewModel = null;
        public IDoorsViewModel DoorsViewModel
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
                _viewModel = value as IDoorsViewModel;
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
