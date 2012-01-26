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

using ApplicationCore;

namespace SpawnGroupPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    /// 
    [AutoRegister]
    public partial class SpawnGroupEditTab : UserControl, ApplicationCore.UserControls.ITabEditorControl
    {
        private readonly SpawnGroupEditTabViewModel _viewModel;

        public SpawnGroupEditTab(SpawnGroupEditTabViewModel viewModel)
        {
            _viewModel = viewModel;

            InitializeComponent();
            DataContext = viewModel;
            EntriesDataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(EntriesDataGrid_AutoGeneratingColumn);
        }

        public SpawnGroupEditTabViewModel ViewModel
        {
            get { return _viewModel; }
        }

        void EntriesDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //TODO use reflection and decorate the properties that should be generated
            var header = e.Column.Header.ToString();

            switch(header)
            {                
                case "Chance":                
                    return;
                case "NpcLevel":
                case "NpcName":
                case "NpcID":
                    e.Column.IsReadOnly = true;
                    return;
                default:
                    e.Cancel = true;                    
                    break;
            }
        }

        public string TabTitle
        {
            get { return "Spawn Groups"; }
        }
    }
}
