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
using System.ComponentModel;

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
            DataContext = _viewModel = viewModel;
            InitializeComponent();

            DataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(DataGrid_AutoGeneratingColumn);

            var categories = viewModel.NpcTemplates.Templates.GroupBy(x => x.Category);
            foreach (var cat in categories)
            {
                var itemCategory = new TreeViewItem();
                itemCategory.Header = cat.Key;

                foreach (var i in cat)
                {
                    var item = new TreeViewItem();
                    item.Header = i.Name;
                    itemCategory.Items.Add(item);
                }
                TreeView.Items.Add(itemCategory);
            }
            TreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(TreeView_SelectedItemChanged);

            viewModel.PropertyChanged += new PropertyChangedEventHandler(viewModel_PropertyChanged);
            viewModel.TemplateAppliedToNpc += new TemplateApplied(viewModel_TemplateAppliedToNpc);

            DataGrid.SelectedCellsChanged += new SelectedCellsChangedEventHandler(DataGrid_SelectedCellsChanged);
        }

        void viewModel_TemplateAppliedToNpc(object sender, TemplateAppliedEventArgs e)
        {
            DataGrid.Items.Refresh();
        }

        void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            NpcEditViewModel.SelectedNpcs = DataGrid.SelectedItems.Cast<EQEmu.Spawns.Npc>();
        }

        void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = e.NewValue as TreeViewItem;
            if (item != null)
            {
                NpcEditViewModel.SelectedTemplate = NpcEditViewModel.NpcTemplates.Templates.Where(x => x.Name == (string)item.Header).FirstOrDefault();
            }
        }

        void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedNpc":
                    OnObjectSelected(NpcEditViewModel.SelectedNpc);
                    break;
                default:
                    break;
            }
        }

        void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var header = e.Column.Header.ToString();
            var pinfo = typeof(EQEmu.Spawns.Npc).GetProperty(header);
            var attr = pinfo.GetCustomAttributes(typeof(BrowsableAttribute), true);
            if (attr.Count() > 0)
            {
                var browseable = attr.ElementAt(0) as BrowsableAttribute;
                if (browseable != null && browseable.Browsable == false) e.Cancel = true;
            }
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

        NpcTypeEditViewModel NpcEditViewModel
        {
            get { return _viewModel as NpcTypeEditViewModel; }
        }

        public event ApplicationCore.UserControls.ObjectSelected ObjectSelected;
        private void OnObjectSelected(object o)
        {
            var e = ObjectSelected;
            if (e != null)
            {
                e(this, new ApplicationCore.UserControls.ObjectSelectedEventArgs(o));
            }
        }
    }
}
