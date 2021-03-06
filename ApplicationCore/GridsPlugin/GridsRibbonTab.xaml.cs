﻿using System;
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

namespace GridsPlugin
{
    /// <summary>
    /// Interaction logic for PathTab.xaml
    /// </summary>
    [AutoRegister]
    public partial class GridsRibbonTab : RibbonTab, IRibbonTab, IGridsControl
    {
        public GridsRibbonTab(GridsRibbonTabViewModel vm)
        {
            InitializeComponent();
            DataContext = _viewModel = vm;
            vm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(vm_PropertyChanged);            
        }

        void vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedGrid":
                    OnObjectSelectionChanged(GridsViewModel.SelectedGrid);
                    break;
                case "SelectedWaypoint":
                    OnObjectSelectionChanged(GridsViewModel.SelectedWaypoint);
                    break;
                default:
                    break;
            }
        }

        private IGridsViewModel _viewModel = null;
        public IGridsViewModel GridsViewModel
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
                _viewModel = value as IGridsViewModel;
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

        private void EditGridButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new PropertyEditorWindow(GridsViewModel.SelectedGrid);
            window.ShowDialog();            

            //TODO hack to update display
            var waypoint = GridsViewModel.SelectedWaypoint;
            var grid = GridsViewModel.SelectedGrid;
            this.GridsViewModel.SelectedGrid = grid;
            this.GridsViewModel.SelectedWaypoint = waypoint;
        }

        private void EditWaypointButton_Click(object sender, RoutedEventArgs e)
        {
            PropertyEditorWindow window = null;
            if (GridsViewModel.SelectedWaypoints != null)
            {
                window = new PropertyEditorWindow(GridsViewModel.SelectedWaypoints);
            }
            else
            {
                window = new PropertyEditorWindow(GridsViewModel.SelectedWaypoint);
            }
            window.ShowDialog();

            //TODO hack to update display... probably need to add an event for when dirtied
            var waypoint = GridsViewModel.SelectedWaypoint;
            var grid = GridsViewModel.SelectedGrid;
            this.GridsViewModel.SelectedGrid = grid;
            this.GridsViewModel.SelectedWaypoint = waypoint;
            GridsViewModel.SelectedWaypoints = GridsViewModel.SelectedWaypoints;
        }

        private void MoveToButton_Click(object sender, RoutedEventArgs e)
        {
            var waypoint = GridsViewModel.SelectedWaypoint;
            Point3D pt;
            if (waypoint != null)
            {
                pt = new Point3D(waypoint.X, waypoint.Y, waypoint.Z);
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

        private void GetQueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (GridsViewModel.GridsService != null && GridsViewModel.GridsService.ZoneGrids != null)
            {
                SaveFileDialog sd = new SaveFileDialog();
                if ((bool)sd.ShowDialog())
                {
                    GridsViewModel.GridsService.ZoneGrids.SaveQueryToFile(sd.FileName);
                }
                return;
            }
        }

        private void RemoveGridButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO use commanding instead
            if (GridsViewModel.SelectedGrid != null && GridsViewModel.GridsService.ZoneGrids != null)
            {
                GridsViewModel.GridsService.ZoneGrids.RemoveGrid(this.GridsViewModel.SelectedGrid);
            }
        }

        private void RemoveWaypointButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO use commanding instead
            if (GridsViewModel.SelectedWaypoint != null && GridsViewModel.SelectedGrid != null
                && GridsViewModel.GridsService.ZoneGrids != null)
            {
                GridsViewModel.SelectedGrid.RemoveWaypoint(GridsViewModel.SelectedWaypoint);
                GridsViewModel.SelectedGrid = GridsViewModel.SelectedGrid;
            }
        }

        private void NewGridButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO use commanding instead
            if (GridsViewModel.GridsService != null && GridsViewModel.GridsService.ZoneGrids != null)
            {
                var grid = GridsViewModel.GridsService.ZoneGrids.GetNewGrid();
                GridsViewModel.GridsService.ZoneGrids.AddGrid(grid);
                GridsViewModel.SelectedGrid = grid;
            }
        }

        private void ViewQueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (GridsViewModel.GridsService != null && GridsViewModel.GridsService.ZoneGrids != null)
            {
                //SaveFileDialog sd = new SaveFileDialog();
                //if ((bool)sd.ShowDialog())
                //{
                //    GridsViewModel.GridsService.ZoneGrids.SaveQueryToFile(sd.FileName);
                //}
                var window = new TextWindow(GridsViewModel.GridsService.ZoneGrids.GetSQL());
                window.Title = "Update Query";
                window.Show();               
                return;
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

        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (GridsViewModel.GridsService != null && GridsViewModel.GridsService.ZoneGrids != null)
            {
                var sd = new SaveFileDialog();
                if ((bool)sd.ShowDialog())
                {
                    GridsViewModel.GridsService.ZoneGrids.SaveXML(System.IO.Path.GetDirectoryName(sd.FileName));
                }
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (GridsViewModel.GridsService != null)
            {
                var sd = new OpenFileDialog();
                sd.Filter = "XML Files | *.grids.xml";
                if ((bool)sd.ShowDialog())
                {
                    //not too fond of this, function call would be easier to read, the setter intializes the zonegrid object
                    GridsViewModel.GridsService.Zone = "";
                    GridsViewModel.GridsService.ZoneGrids.LoadXML(sd.FileName);
                }
            }
        }
    }
}
