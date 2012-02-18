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

using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.UserControls.Ribbon.Tabs;
using ApplicationCore.ViewModels.Editors;

namespace SpawnsPlugin
{
    /// <summary>
    /// Interaction logic for PathTab.xaml
    /// </summary>
    [AutoRegister]
    public partial class SpawnsRibbonTab : RibbonTab, IRibbonTab, ISpawnsControl
    {
        public SpawnsRibbonTab(SpawnsRibbonTabViewModel vm)
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
                _viewModel = value as ISpawnsViewModel;
            }
        }

        private ISpawnsViewModel _viewModel = null;
        public ISpawnsViewModel SpawnsViewModel
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

        [OptionalDependency]
        public ProjectionCamera Camera3D
        {
            get;
            set;
        }

        [OptionalDependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }
        
        private void NewSpawnButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO use commanding
            SpawnsViewModel.CreateNewSpawn(new Point3D(0,0,0));
        }

        private void RemoveSpawnButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO use commanding
            if (SpawnsViewModel.SpawnsService != null && SpawnsViewModel.SpawnsService.ZoneSpawns != null)
            {
                if (SpawnsViewModel.SelectedSpawn != null)
                {
                    SpawnsViewModel.SpawnsService.ZoneSpawns.RemoveSpawn(SpawnsViewModel.SelectedSpawn);
                    SpawnsViewModel.SelectedSpawn = null;
                }

                if (SpawnsViewModel.SelectedSpawns != null)
                {
                    foreach (var s in SpawnsViewModel.SelectedSpawns)
                    {
                        SpawnsViewModel.SpawnsService.ZoneSpawns.RemoveSpawn(s);
                        SpawnsViewModel.SelectedSpawns = null;
                    }
                }
            }
        }

        private void EditSpawnButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO use commanding
            if (SpawnsViewModel.SelectedSpawn != null)
            {
                if (SpawnsViewModel.SelectedSpawns != null)
                {
                    var window = new PropertyEditorWindow(SpawnsViewModel.SelectedSpawns);
                    window.ShowDialog();

                    //TODO fix - hack to update display
                    SpawnsViewModel.SelectedSpawns = SpawnsViewModel.SelectedSpawns;
                }
                else
                {
                    var window = new PropertyEditorWindow(SpawnsViewModel.SelectedSpawn);
                    window.ShowDialog();
                }

                //TODO fix - this is a hack to update the display
                SpawnsViewModel.SelectedSpawn = SpawnsViewModel.SelectedSpawn;
            }
        }

        private void GetUpdateQueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpawnsViewModel.SpawnsService != null && SpawnsViewModel.SpawnsService.ZoneSpawns != null)
            {
                SaveFileDialog sd = new SaveFileDialog();
                if ((bool)sd.ShowDialog())
                {
                    SpawnsViewModel.SpawnsService.ZoneSpawns.SaveQueryToFile(sd.FileName);                    
                }
                return;                
            }
        }

        private void PHPEditorButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpawnsViewModel.SelectedSpawn != null)
            {
                System.Diagnostics.Process.Start(SpawnsViewModel.Service.DBConfiguration.PEQEditorUrl + "index.php?action=59&zoneid=&editor=spawn&z="+ SpawnsViewModel.SpawnsService.Zone + "&spawngroup=" + SpawnsViewModel.SpawnsService.SelectedSpawn.SpawnGroupId);
            }
        }

        private void ViewQueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpawnsViewModel.SpawnsService != null && SpawnsViewModel.SpawnsService.ZoneSpawns != null)
            {
                var window = new TextWindow(SpawnsViewModel.SpawnsService.ZoneSpawns.GetSQL());
                window.ShowDialog();
            }
        }

        private void PackSpawnsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpawnsViewModel.SpawnsService != null && SpawnsViewModel.SpawnsService.ZoneSpawns != null)
            {
                var window = new PackDialog(new PackDialogViewModel(SpawnsViewModel.SpawnsService.ZoneSpawns));
                window.ShowDialog();
            }
        }
    }
}
