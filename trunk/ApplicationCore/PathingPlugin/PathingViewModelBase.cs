using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;
using ApplicationCore.DataServices;

namespace PathingPlugin
{
    public abstract class PathingViewModelBase : EditorViewModelBase, IPathingViewModel
    {
        private bool _pathExists;

        public PathingViewModelBase(PathingDataService service)
        {
            _service = service;
            _service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( service_PropertyChanged );

            AutoConnect = true;
            UseTwoWayConnect = true;
            UseWarpConnect = false;
            ZAdjustment = 2.0;
        }

        public bool PathingExists
        {
            get { return _service.Pathing != null; }
        }

        void service_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            SaveCommand.RaiseCanExecuteChanged();            
            switch ( e.PropertyName ) {
                case "Pathing":
                    NewNodeCommand.RaiseCanExecuteChanged();
                    NotifyPropertyChanged( "NodeCount" );
                    NotifyPropertyChanged( "Nodes" );
                    NotifyPropertyChanged( "DisconnectedNodesCount" );
                    NotifyPropertyChanged("DisconnectedNodes");
                    NotifyPropertyChanged("PathingExists");
                    break;                    
                case "SelectedNode":
                    RemoveNodeCommand.RaiseCanExecuteChanged();
                    NotifyPropertyChanged( "SelectedNode" );
                    NotifyPropertyChanged("DisconnectedNodesCount");
                    NotifyPropertyChanged("DisconnectedNodes");
                    break;
                default:
                    break;
            }
        }

        abstract public override void User3DClickAt(object sender, World3DClickEventArgs e);

        #region New Command

        private DelegateCommand _newCommand;
        public DelegateCommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new DelegateCommand(
                        x => ExecuteNewCommand(x),
                        y => CanExecuteNewCommand(y));
                    NotifyPropertyChanged("NewCommand");
                }
                return _newCommand;
            }

        }        

        abstract public bool CanExecuteNewCommand(object arg);
        abstract public void ExecuteNewCommand(object arg);

        #endregion

        #region Open Command

        private DelegateCommand _openCommand;
        public DelegateCommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                {
                    _openCommand = new DelegateCommand(
                        x => ExecuteOpenCommand(x),
                        y => CanExecuteOpenCommand(y));
                    NotifyPropertyChanged("OpenCommand");
                }
                return _openCommand;
            }
        }
        abstract public bool CanExecuteOpenCommand(object arg);
        abstract public void ExecuteOpenCommand(object arg);

        #endregion

        #region Save Command

        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new DelegateCommand(
                        x => ExecuteSaveCommand(x),
                        y => CanExecuteSaveCommand(y));
                    NotifyPropertyChanged("SaveCommand");
                }
                return _saveCommand;
            }
        }
        abstract public bool CanExecuteSaveCommand(object arg);
        abstract public void ExecuteSaveCommand(object arg);

        #endregion

        #region Node Selected Command

        private DelegateCommand _nodeSelectedCommand;
        public DelegateCommand NodeSelectedCommand
        {
            get
            {
                if (_nodeSelectedCommand == null)
                {
                    _nodeSelectedCommand = new DelegateCommand(ExecuteNodeSelectedCommand, CanNodeSelectedCommand);
                    NotifyPropertyChanged("NodeSelectedCommand");
                }
                return _nodeSelectedCommand;
            }
        }

        abstract public bool CanNodeSelectedCommand(object arg);
        abstract public void ExecuteNodeSelectedCommand(object arg);

        #endregion

        #region New Node Command

        private DelegateCommand _newNodeCommand;
        public DelegateCommand NewNodeCommand
        {
            get
            {
                if (_newNodeCommand == null)
                {
                    _newNodeCommand = new DelegateCommand(ExecuteNewNodeCommand,CanNewNodeCommand);
                    NotifyPropertyChanged("NewNodeCommand");
                }
                return _newNodeCommand;
            }
        }

        abstract public bool CanNewNodeCommand(object arg);
        abstract public void ExecuteNewNodeCommand(object arg);

        #endregion

        #region Remove Node Command

        private DelegateCommand _removeNodeCommand;
        public DelegateCommand RemoveNodeCommand
        {
            get
            {
                if (_removeNodeCommand == null)
                {
                    _removeNodeCommand = new DelegateCommand(ExecuteRemoveNodeCommand, CanRemoveNodeCommand);
                    NotifyPropertyChanged("RemoveNodeCommand");
                }
                return _removeNodeCommand;
            }
        }

        abstract public bool CanRemoveNodeCommand(object arg);
        abstract public void ExecuteRemoveNodeCommand(object arg);

        #endregion

        public int NodeCount
        {
            get
            {
                if ( _service != null && _service.Pathing != null ) {
                    return _service.Pathing.Nodes.Count;
                } else return 0;
            }
        }

        public int DisconnectedNodesCount
        {
            get
            {
                if ( _service != null && _service.Pathing != null ) {
                    return _service.Pathing.DisconnectedNodes.Count;
                } else return 0;
            }
        }

        public bool UseTwoWayConnect
        {
            get;
            set;
        }

        public bool UseWarpConnect
        {
            get;
            set;
        }

        public double ZAdjustment
        {
            get;
            set;
        }

        public bool AutoConnect
        {
            get;
            set;
        }

        public ObservableCollection<EQEmu.Path.Node> Nodes
        {
            get
            {
                if ( _service != null && _service.Pathing != null ) {
                    return _service.Pathing.Nodes;
                } else return null;
            }
        }

        public ObservableCollection<EQEmu.Path.Node> DisconnectedNodes
        {
            get
            {
                if (_service != null && _service.Pathing != null)
                {
                    return _service.Pathing.DisconnectedNodes;
                }
                else return null;
            }
        }

        public EQEmu.Path.Node SelectedNode
        {
            get
            {
                if ( _service != null ) {
                    return _service.SelectedNode;
                } else return null;
            }
            set
            {
                if ( _service != null ) {
                    _service.SelectedNode = value;                    
                    NotifyPropertyChanged( "SelectedNode" );
                    NotifyPropertyChanged("UnreachableNodes");
                    NodeSelectedCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<EQEmu.Path.Node> UnreachableNodes
        {
            get
            {
                if (_service != null && _service.Pathing != null && SelectedNode != null)
                {
                    return _service.Pathing.GetInaccessibleNodes(SelectedNode);
                }
                else return null;
            }
        }

        private readonly PathingDataService _service = null;
        public PathingDataService PathingService
        {
            get { return _service; }
        }

        public override IDataService Service
        {
            get { return _service; }
        }
    }
}
