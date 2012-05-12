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
            NPCsDataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(NPCsDataGrid_AutoGeneratingColumn);                        
        }

        void NPCsDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var header = e.Column.Header.ToString();

            switch (header)
            {
                case "Name":
                case "Level":
                case "Id":
                    e.Column.IsReadOnly = true;
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
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

        private void AddNPCButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedSpawnGroup != null)
            {
                foreach (EQEmu.Spawns.NPC npc in NPCsDataGrid.SelectedItems)
                {
                    var entry = ViewModel.SelectedSpawnGroup.CreateEntry();
                    entry.Chance = 0;
                    entry.NpcID = npc.Id;
                    entry.SpawnGroupID = ViewModel.SelectedSpawnGroup.Id;
                    entry.NpcName = npc.Name;
                    entry.NpcLevel = (short)npc.Level;
                    entry.Created();
                    ViewModel.SelectedSpawnGroup.AddEntry(entry);
                    ViewModel.RefreshSelection();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.AdjustChanceTotalCommand.CanExecute(EntriesDataGrid.SelectedItems))
            {
                ViewModel.AdjustChanceTotalCommand.Execute(EntriesDataGrid.SelectedItems);
                EntriesDataGrid.Items.Refresh();
            }
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var sd = new SaveFileDialog();            
            if ((bool)sd.ShowDialog())
            {
                string zone = "";
                if (ViewModel.ZoneFilter != null) zone = ViewModel.ZoneFilter;
                ViewModel.SaveAsXml(zone, System.IO.Path.GetDirectoryName(sd.FileName) );
            }
        }
    }
}
