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
        public MapViewModelBase(MapDataService service)
        {
            _service = service;
        }

        public abstract override void User3DClickAt( object sender, World3DClickEventArgs e );

        public DelegateCommand _openCommand;
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


        public override IDataService Service { get { return _service; } }

        private readonly MapDataService _service = null;
        public MapDataService MapService
        {
            get { return _service; }
        }
    }
}
