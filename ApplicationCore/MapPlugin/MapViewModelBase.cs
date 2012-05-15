using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Practices.Unity;

using ApplicationCore;
using ApplicationCore.ViewModels.Editors;
using ApplicationCore.DataServices;

namespace MapPlugin
{
    abstract public class MapViewModelBase : EditorViewModelBase, IMapViewModel
    {
        private DelegateCommand _closeCommand;
        public DelegateCommand _openCommand;

        public MapViewModelBase(MapDataService service)
        {
            _service = service;
        }

        public abstract override void User3DClickAt( object sender, World3DClickEventArgs e );
        
        public DelegateCommand OpenCommand
        {
            get
            {
                if( _openCommand == null )
                {
                    _openCommand = new DelegateCommand( 
                        x => ExecuteOpenCommand(x),
                        y => CanExecuteOpenCommand(y) );
                }
                return _openCommand;
            }
        }
        abstract public bool CanExecuteOpenCommand(object arg);
        abstract public void ExecuteOpenCommand(object arg);

        public DelegateCommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new DelegateCommand(
                        x => ExecuteCloseCommand(x),
                        y => CanExecuteCloseCommand(y));
                }
                return _closeCommand;
            }
        }

        abstract public bool CanExecuteCloseCommand(object arg);
        abstract public void ExecuteCloseCommand(object arg);


        public override IDataService Service { get { return _service; } }

        private readonly MapDataService _service = null;
        public MapDataService MapService
        {
            get { return _service; }
        }
    }
}
