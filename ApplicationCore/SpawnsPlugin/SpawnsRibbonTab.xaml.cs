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
using System.IO;

using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore;
using ApplicationCore.UserControls;
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

            vm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(vm_PropertyChanged);
        }

        void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedSpawn":
                    OnObjectSelectionChanged(SpawnsViewModel.SelectedSpawn);
                    break;
                default:
                    break;
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
                    foreach (var s in SpawnsViewModel.SelectedSpawns.ToArray())
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


        public event ApplicationCore.UserControls.ObjectSelected ObjectSelected;
        private void OnObjectSelectionChanged(object obj)
        {
            var e = ObjectSelected;
            if (e != null)
            {
                e(this, new ApplicationCore.UserControls.ObjectSelectedEventArgs(obj));
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var path = System.Environment.CurrentDirectory + "\\Help\\spawns-plugin.chm";
            if (File.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }
        }

        private void MoveToButton_Click(object sender, RoutedEventArgs e)
        {
            if (Camera3D != null)
            {
                var sel = SpawnsViewModel.SelectedSpawn;
                Point3D pt;
                if (sel != null)
                {
                    pt = new Point3D(sel.X, sel.Y, sel.Z);
                    if (Transform3D != null)
                    {
                        Transform3D.TryTransform(pt, out pt);
                    }
                }
                else return;

                if (Camera3D != null)
                {
                    Camera3D.Position = pt;
                }
            }
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            var sd = new System.Windows.Forms.FolderBrowserDialog();
            sd.Description = "Choose directory to save to";
            if (sd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SpawnsViewModel.SaveToFile(sd.SelectedPath);
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.Filter = "XML Files | *.spawn2.xml";
            if ((bool)fd.ShowDialog())
            {
                SpawnsViewModel.LoadFile(fd.FileName);
            }
        }
    }
}
