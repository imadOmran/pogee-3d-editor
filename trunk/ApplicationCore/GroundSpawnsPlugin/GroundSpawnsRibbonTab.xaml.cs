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

using Microsoft.Practices.Unity;
using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Win32;

using ApplicationCore.UserControls.Ribbon.Tabs;
using ApplicationCore;
using ApplicationCore.UserControls;
using ApplicationCore.ViewModels.Editors;

namespace GroundSpawnsPlugin
{
    /// <summary>
    /// Interaction logic for PathTab.xaml
    /// </summary>
    [AutoRegister]
    public partial class GroundSpawnsRibbonTab : RibbonTab, IRibbonTab, IGroundSpawnsControl
    {
        public GroundSpawnsRibbonTab(GroundSpawnsRibbonTabViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
            vm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(vm_PropertyChanged);
        }

        void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedGroundSpawn":
                    OnObjectSelectionChanged(GroundSpawnsViewModel.SelectedGroundSpawn);
                    break;
                default:
                    break;
            }
        }

        private IGroundSpawnsViewModel _viewModel = null;
        public IGroundSpawnsViewModel GroundSpawnsViewModel
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
                _viewModel = value as IGroundSpawnsViewModel;
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

        private void GetQueryButton_Click(object sender, RoutedEventArgs e)
        {
            //if (GroundSpawnsViewModel.Service != null &&  .ZoneGrids != null)
            //{
            //    SaveFileDialog sd = new SaveFileDialog();
            //    if ((bool)sd.ShowDialog())
            //    {
            //        GridsViewModel.GridsService.ZoneGrids.SaveQueryToFile(sd.FileName);
            //    }
            //    return;
            //}
        }

        private void ViewQueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (GroundSpawnsViewModel.GroundSpawnsService != null)
            {
                var window =
                    new TextWindow(
                        GroundSpawnsViewModel.GroundSpawnsService.ZoneGroundSpawns.GetSQL());
                window.Show();
            }
        }


        private void EditGroundSpawnButton_Click(object sender, RoutedEventArgs e)
        {
            if (GroundSpawnsViewModel.SelectedGroundSpawn != null)
            {
                var selSpawn = GroundSpawnsViewModel.SelectedGroundSpawn;

                if (GroundSpawnsViewModel.SelectedGroundSpawns != null)
                {
                    List<EQEmu.GroundSpawns.GroundSpawn> spawns = new List<EQEmu.GroundSpawns.GroundSpawn>();
                    spawns.Add(selSpawn);
                    foreach (var sp in GroundSpawnsViewModel.SelectedGroundSpawns.Where(x => { return x != selSpawn; }))
                    {
                        spawns.Add(sp);
                    }

                    var window = new PropertyEditorWindow(spawns);
                    window.ShowDialog();
                }
                else
                {
                    var window = new PropertyEditorWindow(GroundSpawnsViewModel.GroundSpawnsService.SelectedGroundSpawn);
                    window.ShowDialog();
                }
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
