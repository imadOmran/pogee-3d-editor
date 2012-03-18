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

namespace LootPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    /// 
    [AutoRegister]
    public partial class LootEditTab : UserControl, ApplicationCore.UserControls.ITabEditorControl
    {
        public LootEditTab(LootEditTabViewModel viewmodel)
        {
            InitializeComponent();
            ViewModel = viewmodel;
            viewmodel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(viewmodel_PropertyChanged);

            ItemLookupDataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(ItemLookupDataGrid_AutoGeneratingColumn);
            TableLookupDataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(ItemLookupDataGrid_AutoGeneratingColumn);
            NPCLookupDataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(NPCLookupDataGrid_AutoGeneratingColumn);
            LootDropDataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(LootDropDataGrid_AutoGeneratingColumn);
            DropEntryDataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(DropEntryDataGrid_AutoGeneratingColumn);
            LootTableDataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(LootTableDataGrid_AutoGeneratingColumn);
        }

        void LootTableDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var header = e.Column.Header.ToString();
            switch (header)
            {
                case "MinCash":
                case "MaxCash":
                case "Name":
                    break;
                case "Id":
                    e.Column.IsReadOnly = true;
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }

        void DropEntryDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var header = e.Column.Header.ToString();
            switch (header)
            {
                case "Chance":
                case "EquipItem":
                case "ItemCharges":
                case "ItemId":
                    break;
                case "ItemName":
                    e.Column.IsReadOnly = true;
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }

        void LootDropDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var header = e.Column.Header.ToString();
            switch (header)
            {
                case "Id":                
                    e.Column.IsReadOnly = true;
                    break;
                case "Name":
                case "Probability":
                case "Multiplier":
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }

        void NPCLookupDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var header = e.Column.Header.ToString();

            switch (header)
            {
                case "Name":
                case "Level":
                case "Id":
                case "LootTableId":
                    e.Column.IsReadOnly = true;
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }

        void ItemLookupDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.IsReadOnly = true;
        }

        void viewmodel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedLootTable":
                    if (ViewModel.SelectedLootTable != null)
                    {
                        OnObjectSelectionChanged(ViewModel.SelectedLootTable);
                    }
                    break;
                case "SelectedLootDrop":
                    if (ViewModel.SelectedLootDrop != null)
                    {
                        OnObjectSelectionChanged(ViewModel.SelectedLootDrop);
                    }
                    break;
                case "SelectedDropEntry":
                    if (ViewModel.SelectedDropEntry != null)
                    {
                        OnObjectSelectionChanged(ViewModel.SelectedDropEntry);
                    }
                    break;
            }
        }

        private LootEditTabViewModel _viewModel;
        public LootEditTabViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                DataContext = value;
            }
        }

        public string TabTitle
        {
            get { return "Loot"; }
        }

        ApplicationCore.ViewModels.Editors.IEditorViewModel ApplicationCore.UserControls.IEditorControl.ViewModel
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (dynamic item in TableLookupDataGrid.SelectedItems.Cast<dynamic>())
            {
                _viewModel.FilterId = item.Id;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (dynamic item in NPCLookupDataGrid.SelectedItems)
            {
                _viewModel.FilterId = item.LootTableId;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var sql = _viewModel.GetSQL();            
            var window = new TextWindow(sql);
            window.ShowDialog();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedLootDrop == null) return;
            foreach (var item in ItemLookupDataGrid.SelectedItems.Cast<EQEmu.Loot.Item>())
            {
                var entry = ViewModel.SelectedLootDrop.Create();
                entry.ItemId = item.Id;
                entry.ItemName = item.Name;
                ViewModel.SelectedLootDrop.AddLootDropEntry(entry);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedLootTable == null) return;
            foreach (var item in LootDropDataGrid.SelectedItems.Cast<EQEmu.Loot.LootDrop>().ToArray())
            {
                ViewModel.SelectedLootTable.RemoveLootDrop(item);
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedLootDrop == null) return;
            var sql = ViewModel.SelectedLootDrop.GetSQL();
            var window = new TextWindow(sql);
            window.ShowDialog();
        }

        private void BalanceDropEntriesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedLootDrop == null) return;

            if (DropEntryDataGrid.SelectedItems.Count <= 0) return;

            ViewModel.SelectedLootDrop.BalanceEntries(DropEntryDataGrid.SelectedItems.Cast<EQEmu.Loot.LootDropEntry>());
            DropEntryDataGrid.Items.Refresh();
        }

        private void AddLootDropToNPC(object sender, RoutedEventArgs e)
        {
            if( ViewModel.SelectedLootDrop == null ) return;
            foreach (var npc in NPCLookupDataGrid.SelectedItems.Cast<EQEmu.Spawns.NPC>())
            {
                //first load the loottable into the aggregator
                ViewModel.LookupLootTable(npc.LootTableId);
                var table = 
                    ViewModel.Cache.Cast<EQEmu.Loot.LootTable>().FirstOrDefault(x => x.Id == npc.LootTableId);
                //if it found a loottable associated with the selected npc add the selected loot drop
                if (table != null)
                {
                    int chance = 0;
                    Int32.TryParse(AddDropNpcChance.Text,out chance);

                    var dropcopy = ViewModel.SelectedLootDrop.Clone();
                    dropcopy.LootTableId = table.Id;
                    dropcopy.Probability = chance;
                    table.AddLootDrop(dropcopy);
                }
            }
        }

        private void SetNPCLootTable(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedLootTable == null) return;
            foreach (var npc in NPCLookupDataGrid.SelectedItems.Cast<EQEmu.Spawns.NPC>())
            {
                npc.LootTableId = ViewModel.SelectedLootTable.Id;
            }

            NPCLookupDataGrid.Items.Refresh();
        }
    }
}
