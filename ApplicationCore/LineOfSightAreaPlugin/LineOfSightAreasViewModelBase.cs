using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore.ViewModels.Editors;
using ApplicationCore.DataServices;
using ApplicationCore;

namespace LineOfSightAreaPlugin
{
    abstract public class LineOfSightAreasViewModelBase : EditorViewModelBase, ILineOfSightAreasViewModel
    {
        public LineOfSightAreasViewModelBase(LineOfSightAreaDataService service)
        {
            _service = service;
            service.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(service_PropertyChanged);
        }

        void service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ZoneAreas":
                    SaveCommand.RaiseCanExecuteChanged();
                    NotifyPropertyChanged("Areas");
                    NewAreaCommand.RaiseCanExecuteChanged();
                    break;
                case "SelectedArea":
                    NotifyPropertyChanged("SelectedArea");
                    break;
                default:
                    break;
            }
            //throw new NotImplementedException();
        }

        private readonly LineOfSightAreaDataService _service = null;
        public LineOfSightAreaDataService LineOfSightAreasService
        {
            get { return _service; }
        }

        abstract public override void User3DClickAt(object sender, World3DClickEventArgs e);

        public override IDataService Service
        {
            get { return _service; }
        }

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

        #region New Area Command

        private DelegateCommand _newAreaCommand;
        public DelegateCommand NewAreaCommand
        {
            get
            {
                if (_newAreaCommand == null)
                {
                    _newAreaCommand = new DelegateCommand(
                        x => ExecuteNewAreaCommand(x),
                        y => CanExecuteNewAreaCommand(y));
                    NotifyPropertyChanged("NewAreaCommand");
                }
                return _newAreaCommand;
            }
        }
        abstract public bool CanExecuteNewAreaCommand(object arg);
        abstract public void ExecuteNewAreaCommand(object arg);

        #endregion

        #region Remove Area Command

        private DelegateCommand _removeAreaCommand;
        public DelegateCommand RemoveAreaCommand
        {
            get
            {
                if (_removeAreaCommand == null)
                {
                    _removeAreaCommand = new DelegateCommand(
                        x => ExecuteRemoveAreaCommand(x),
                        y => CanExecuteRemoveAreaCommand(y));
                    NotifyPropertyChanged("RemoveAreaCommand");
                }
                return _removeAreaCommand;
            }
        }
        abstract public bool CanExecuteRemoveAreaCommand(object arg);
        abstract public void ExecuteRemoveAreaCommand(object arg);

        #endregion


        public EQEmu.LineOfSightAreas.LineOfSightArea SelectedArea
        {
            get
            {
                if (LineOfSightAreasService != null)
                {
                    return LineOfSightAreasService.SelectedArea;
                }
                else return null;
            }
            set
            {
                LineOfSightAreasService.SelectedArea = value;
                RemoveAreaCommand.RaiseCanExecuteChanged();
            }
        }

        public System.Windows.Media.Media3D.Point3D? SelectedVertex
        {
            get
            {
                if (LineOfSightAreasService != null)
                {
                    return LineOfSightAreasService.SelectedVertex;
                }
                else return null;
            }
            set
            {
                if (LineOfSightAreasService != null && value.HasValue)
                {
                    LineOfSightAreasService.SelectedVertex = value.Value;
                    NotifyPropertyChanged("SelectedVertex");
                }
            }
        }


        public System.Collections.ObjectModel.ObservableCollection<EQEmu.LineOfSightAreas.LineOfSightArea> Areas
        {
            get
            {
                if (LineOfSightAreasService != null && LineOfSightAreasService.ZoneAreas != null)
                {
                    return LineOfSightAreasService.ZoneAreas.Areas;
                }
                else return null;
            }
        }


        abstract public void NewArea();
    }
}
