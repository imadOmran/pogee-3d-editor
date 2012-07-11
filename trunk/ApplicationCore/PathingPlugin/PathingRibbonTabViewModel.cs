using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Input;

using Microsoft.Practices.Unity;
using Microsoft.Win32;

using ApplicationCore.ViewModels.Editors;

namespace PathingPlugin
{
    public class PathingRibbonTabViewModel : PathingViewModelBase
    {
        public PathingRibbonTabViewModel([Dependency("PathingDataService")] PathingDataService _service)
            : base(_service)
        {        
        }
        
        public void OpenFile()
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Pathing Files (.path)|*.path|All Files (*.*)|*.*";
            if ((bool)fd.ShowDialog())
            {
                PathingService.OpenFile(fd.FileName);
            }
        }

        public void NewFile()
        {
            PathingService.NewFile();
        }

        [OptionalDependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get;
            set;
        }

        protected override void OnLeftMouseClick(object sender, World3DClickEventArgs e)
        {
            base.OnLeftMouseClick(sender, e);
            if (PathingService != null && PathingService.Pathing != null)
            {
                Point3D p = new Point3D(e.PointInWorld.X, e.PointInWorld.Y, e.PointInWorld.Z);
                if (Transform3D != null)
                {
                    Transform3D.TryTransform(p, out p);
                }


                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    var newnode = new EQEmu.Path.Node(p.X, p.Y, p.Z + ZAdjustment);
                    if (PathingService != null && PathingService.Pathing != null)
                    {
                        PathingService.Pathing.AddNode(newnode);
                        if (SelectedNode != null && AutoConnect)
                        {
                            SelectedNode.ConnectToNodeTwoWay(newnode);
                            SelectedNode = newnode;
                        }
                    }
                    return;
                }

                if (Keyboard.IsKeyDown(Key.LeftAlt) && SelectedNode != null)
                {
                    SelectedNode.X = p.X;
                    SelectedNode.Y = p.Y;
                    SelectedNode.Z = p.Z + ZAdjustment;
                    return;
                }

                var node = PathingService.Pathing.GetNearbyNode(p);
                if (node != null)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) && SelectedNode != null)
                    {
                        if (UseTwoWayConnect)
                        {
                            if (UseWarpConnect)
                            {
                                SelectedNode.ConnectToNodeAsWarp(node);
                                node.ConnectToNodeAsWarp(SelectedNode);
                            }
                            else
                            {
                                SelectedNode.ConnectToNodeTwoWay(node);
                            }
                        }
                        else
                        {
                            if (UseWarpConnect)
                            {
                                SelectedNode.ConnectToNodeAsWarp(node);
                            }
                            else
                            {
                                SelectedNode.ConnectToNode(node);
                            }
                        }
                        return;
                    }

                    SelectedNode = node;
                }
            }
        }

        public override void User3DClickAt(object sender, World3DClickEventArgs e)
        {
            if (e.ActiveRibbonControl as IPathingControl == null) return;
            base.User3DClickAt(sender, e);
        }

        public override bool CanExecuteOpenCommand(object arg)
        {
            return true;
        }

        public override void ExecuteOpenCommand(object arg)
        {
            OpenFile();
        }

        public override bool CanExecuteNewCommand(object arg)
        {
            return true;
        }

        public override void ExecuteNewCommand(object arg)
        {
            NewFile();
        }

        #region Save Command

        public override bool CanExecuteSaveCommand(object arg)
        {
            return (this.PathingService != null && this.PathingService.Pathing != null);
        }

        public override void ExecuteSaveCommand(object arg)
        {
            SaveFile();
        }

        public void SaveFile()
        {
            SaveFileDialog sd = new SaveFileDialog();
            if ((bool)sd.ShowDialog())
            {
                PathingService.SaveFile(sd.FileName);
            }
        }

        #endregion


        public override bool CanNodeSelectedCommand(object arg)
        {
            return (this.PathingService != null && this.PathingService.SelectedNode != null);
        }

        public override void ExecuteNodeSelectedCommand(object arg)
        {
            //throw new NotImplementedException();
        }

        public override bool CanNewNodeCommand(object arg)
        {
            return (this.PathingService != null && this.PathingService.Pathing != null);
        }

        public override void ExecuteNewNodeCommand(object arg)
        {
            PathingService.Pathing.AddNode(new EQEmu.Path.Node());
        }

        public override bool CanRemoveNodeCommand(object arg)
        {
            return (SelectedNode != null);
        }

        public override void ExecuteRemoveNodeCommand(object arg)
        {
            PathingService.Pathing.RemoveNode(SelectedNode);
        }
    }
}
