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

namespace MapPlugin
{
    /// <summary>
    /// Interaction logic for PathTab.xaml
    /// </summary>
    /// 
    [AutoRegister]
    public partial class MapRibbonTab : RibbonTab, IRibbonTab, IMapControl
    {
        public MapRibbonTab(MapRibbonTabViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
        }

        #region IMapControl Members

        private IMapViewModel _viewModel = null;        
        public IMapViewModel MapViewModel
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

        #endregion

        #region IEditorControl Members

        public IEditorViewModel ViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                _viewModel = value as IMapViewModel;
            }
        }

        #endregion


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
