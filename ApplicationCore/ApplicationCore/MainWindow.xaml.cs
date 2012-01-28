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
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Practices.Unity;

namespace ApplicationCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private Point3D _rightClickPoint;

        public MainWindow()
        {
            this.WindowState = System.Windows.WindowState.Maximized;
            InitializeComponent();
            View3D.PanGesture = new MouseGesture()
            {
                MouseAction = MouseAction.MiddleClick
            };            
            //MainWindowViewModel vm = DataContext as MainWindowViewModel;            
            var vm = DataContext as MainWindowViewModel;            
            View3D.Camera = vm.Container.Resolve<ProjectionCamera>();
            View3D.CameraInertiaFactor = 0.2;

            var tabControls = vm.ResolveTabEditorControls();
            foreach (var c in tabControls)
            {
                MainTabControl.Items.Add(
                    new TabItem() { Header = c.TabTitle, Content = c });
            }

            //initializes the camera - there's probably a better spot to do this
            this.Activated += new EventHandler(MainWindow_Activated);
        }
        
        void MainWindow_Activated(object sender, EventArgs e)
        {            
            View3D.ResetCamera();
            this.Activated -= MainWindow_Activated;
        }

        private void View3D_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var pt = View3D.FindNearestPoint(e.GetPosition(View3D));
            if (!pt.HasValue)
            {
                return;
            }

            Point3D point = (Point3D)pt;

            if (e.ChangedButton == MouseButton.Right)
            {
                View3D.Camera.Position = new Point3D(point.X, point.Y, point.Z + 5);
            }

            var vm = DataContext as MainWindowViewModel;

            if (e.ChangedButton == MouseButton.Left)
            {
                vm.WorldMouseClickAt(point, ribbon.SelectedItem, null);
            }
        }

        private void View3D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool selectionBox = _selectionInProgress;
            UpdateSelectionRectangle();            
            HideSelectionRectangle();

            if (_selectionBoxEndPoint == _selectionBoxStartPoint)
            {
                selectionBox = false;
            }

            var pt = View3D.FindNearestPoint(e.GetPosition(View3D));
            /*
            if (!pt.HasValue)
            {
                return;
            }
            */
            Point3D point = new Point3D();
            if (pt.HasValue)
            {
                point = (Point3D)pt;
            }

            //MainWindowViewModel vm = DataContext as MainWindowViewModel;            
            //vm.MouseClickAt(point,ribbon.SelectedItem);            

            var vm = DataContext as MainWindowViewModel;


            Func<Point3D,double, bool> action = (Func<Point3D,double,bool>)((x,y) =>
                {
                    y = -1.0;
                    return false;
                });

            if (selectionBox)
            {
                double largeX = _selectionBoxEndPoint.X > _selectionBoxStartPoint.X ? _selectionBoxEndPoint.X : _selectionBoxStartPoint.X;
                double smallX = largeX == _selectionBoxEndPoint.X ? _selectionBoxStartPoint.X : _selectionBoxEndPoint.X;
                double largeY = _selectionBoxEndPoint.Y > _selectionBoxStartPoint.Y ? _selectionBoxEndPoint.Y : _selectionBoxStartPoint.X;
                double smallY = largeY == _selectionBoxEndPoint.Y ? _selectionBoxStartPoint.Y : _selectionBoxEndPoint.Y;

                action = (point3D,distance) =>
                {
                    var p2d = HelixToolkit.Wpf.Viewport3DHelper.Point3DtoPoint2D(View3D.Viewport,point3D);

                    if (p2d.X <= largeX && p2d.X >= smallX &&
                        p2d.Y <= largeY && p2d.Y >= smallY)
                    {
                        if (distance > 0)
                        {
                            var hitTestPoint = View3D.FindNearestPoint(p2d);
                            if (hitTestPoint != null)
                            {
                                if (Helpers.Math.Distance(point3D, (Point3D)hitTestPoint) <= distance)
                                {
                                    return true;
                                }
                                else return false;
                            }
                            else return false;
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };
            }

            vm.WorldMouseClickAt(point, ribbon.SelectedItem, new ViewModels.Editors.IsPointInsideSelectionBox(action));

            //vm.WorldMouseClickAt(point, ribbon.SelectedItem,
            //    (point3D) =>
            //    {
            //        var p2d = HelixToolkit.Wpf.Viewport3DHelper.Point3DtoPoint2D(View3D.Viewport,point3D);

            //        if (p2d.X <= largeX && p2d.X >= smallX &&
            //            p2d.Y <= largeY && p2d.Y >= smallY)
            //        {
            //            return true;
            //        }
            //        else
            //        {
            //            return false;
            //        }
            //    });
        }

        private void ViewClippingButton_Click(object sender, RoutedEventArgs e)
        {
            ViewClippingPopup.IsOpen = true;
        }
        
        private void ViewClippingFieldButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel; 
            Button button = sender as Button;
            if (button == null) return;

            Point3D point = new Point3D(View3D.Camera.Position.X,View3D.Camera.Position.Y,View3D.Camera.Position.Z);
            vm.Container.Resolve<Transform3D>("WorldTransform").TryTransform(View3D.Camera.Position, out point);

            switch (button.Name)
            {
                case "ViewClippingXMax":
                    vm.ViewClipping.XMax = (float)point.X;
                    break;
                case "ViewClippingXMin":
                    vm.ViewClipping.XMin = (float)point.X;
                    break;
                case "ViewClippingYMin":
                    vm.ViewClipping.YMin = (float)point.Y;
                    break;
                case "ViewClippingYMax":
                    vm.ViewClipping.YMax = (float)point.Y;
                    break;
                case "ViewClippingZMin":
                    vm.ViewClipping.ZMin = (float)point.Z;
                    break;
                case "ViewClippingZMax":
                    vm.ViewClipping.ZMax = (float)point.Z;
                    break;
                default:
                    return;
            }
            vm.ViewClipping = vm.ViewClipping;
        }

        private void ResetViewClippingButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            vm.ViewClipping.XMax = 0;
            vm.ViewClipping.XMin = 0;
            vm.ViewClipping.YMax = 0;
            vm.ViewClipping.YMin = 0;
            vm.ViewClipping.ZMax = 0;
            vm.ViewClipping.ZMin = 0;
            vm.ViewClipping = vm.ViewClipping;
        }

        private void DatabaseConnectRibbonButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new DatabaseConfigWindow();
            window.ShowDialog();
        }

        private void TransformPoint(ref Point3D p)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm != null)
            {
                vm.Container.Resolve<Transform3D>("WorldTransform").TryTransform(View3D.Camera.Position, out p);                
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private Point _selectionBoxStartPoint;
        private Point _selectionBoxEndPoint;
        private bool _selectionInProgress = false;
        
        private void View3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectionInProgress == true)
            {
                UpdateSelectionRectangle();
                _selectionInProgress = false;
            }

            var pos = Mouse.GetPosition(View3D);
            var margin = SelectionRectangle.Margin;
            margin.Left = pos.X;
            margin.Top = pos.Y;
            SelectionRectangle.Margin = margin;
            SelectionRectangle.Width = 0;
            SelectionRectangle.Height = 0;

            _selectionBoxStartPoint = pos;
            _selectionInProgress = true;
        }

        private void View3D_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_selectionInProgress) return;
            UpdateSelectionRectangle();
        }

        private void UpdateSelectionRectangle()
        {
            var pos = _selectionBoxEndPoint = Mouse.GetPosition(View3D);
            var width = Math.Abs(_selectionBoxStartPoint.X - pos.X);
            var height = Math.Abs(_selectionBoxStartPoint.Y - pos.Y);

            var margin = SelectionRectangle.Margin;
            if (pos.X < _selectionBoxStartPoint.X)
            {
                margin.Left = pos.X;                
            }
            if (pos.Y < _selectionBoxStartPoint.Y)
            {
                margin.Top = pos.Y;
            }

            SelectionRectangle.Width = width;
            SelectionRectangle.Height = height;
            SelectionRectangle.Margin = margin;
        }

        private void HideSelectionRectangle()
        {
            SelectionRectangle.Height = 0;
            SelectionRectangle.Width = 0;
            var margin = SelectionRectangle.Margin;
            margin.Left = 0;
            margin.Top = 0;
            SelectionRectangle.Margin = margin;
            _selectionInProgress = false;
        }
    }
}
