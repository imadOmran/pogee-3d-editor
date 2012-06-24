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
using System.Reflection;
using System.ComponentModel;

using Microsoft.Win32;

using ApplicationCore;

namespace SpawnExtractorPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    /// 
    [AutoRegister]
    public partial class SpawnExtractorTab : UserControl, ApplicationCore.UserControls.ITabEditorControl
    {
        public SpawnExtractorTab(SpawnExtractorTabViewModel viewmodel)
        {
            InitializeComponent();

            ViewModel = viewmodel;
            viewmodel.FileSelectionChanged += new FileLoadingHandler(viewmodel_FileSelectionChanged);


            NPCDataGrid.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(NPCDataGrid_AutoGeneratingColumn);            
        }

        void viewmodel_FileSelectionChanged(object sender, FileLoadingEventArgs e)
        {
            switch (e.State)
            {
                case FileLoadingEventArgs.LoadingState.PreLoad:
                    InputBox.Visibility = System.Windows.Visibility.Visible;
                    break;
                case FileLoadingEventArgs.LoadingState.Loaded:
                    InputBox.Visibility = System.Windows.Visibility.Collapsed;
                    NPCDataGrid.Items.Refresh();
                    break;
                case FileLoadingEventArgs.LoadingState.Error:
                    MessageBox.Show("Could not load file " + e.FileName + ":" + e.Error);
                    break;
            }            
        }

        void NPCDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
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
            get { return "Spawn Extractor"; }
        }

        public ApplicationCore.ViewModels.Editors.IEditorViewModel ViewModel
        {
            get
            {
                return DataContext as ApplicationCore.ViewModels.Editors.IEditorViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        public SpawnExtractorTabViewModel EditorViewModel
        {
            get { return ViewModel as SpawnExtractorTabViewModel; }
        }

        public event ApplicationCore.UserControls.ObjectSelected ObjectSelected;

        private void NPCQueryButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new TextWindow(EditorViewModel.NPCQuery());
            window.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if( NPCDataGrid.SelectedItems.Count > 1 )
            {
                var window = new PropertyEditorWindow(NPCDataGrid.SelectedItems.Cast<EQEmu.Spawns.Npc>());
                window.ShowDialog();
                NPCDataGrid.Items.Refresh();
            }
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (NPCDataGrid.SelectedItems.Count > 0)
            {
                if (EditorViewModel.ApplyTemplateCommand.CanExecute(null))
                {
                    EditorViewModel.ApplyTemplateCommand.Execute(NPCDataGrid.SelectedItems.Cast<EQEmu.Spawns.Npc>());
                    NPCDataGrid.Items.Refresh();
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int num = EditorViewModel.StartId;
            foreach (var npc in NPCDataGrid.SelectedItems.Cast<EQEmu.Spawns.Npc>())
            {
                npc.Id = num++;
            }
        }
    }
}
