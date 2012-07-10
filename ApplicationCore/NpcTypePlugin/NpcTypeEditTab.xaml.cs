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

using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;

namespace NpcTypePlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    /// 
    [AutoRegister]
    public partial class NpcTypeEditTab : UserControl, ApplicationCore.UserControls.ITabEditorControl
    {
        private IEditorViewModel _viewModel;

        public NpcTypeEditTab(NpcTypeEditViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        public string TabTitle
        {
            get { return "NPCs"; }
        }

        IEditorViewModel ApplicationCore.UserControls.IEditorControl.ViewModel
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

        public event ApplicationCore.UserControls.ObjectSelected ObjectSelected;
    }
}
