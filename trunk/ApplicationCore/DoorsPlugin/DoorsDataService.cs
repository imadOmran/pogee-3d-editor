using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.IO;

using Microsoft.Practices.Unity;

using MySql.Data.MySqlClient;

using ApplicationCore;
using ApplicationCore.DataServices;

using EQEmu.Doors;
using EQEmu.Files.S3D;
using EQEmu.Files.WLD;
using EQEmuDisplay3D;

namespace DoorsPlugin
{
    [AutoRegister]    
    public class DoorsDataService : DataServiceBase, IModel3DProvider
    {
        private readonly System.Windows.Threading.Dispatcher _dispatcher = null;
        private DoorsDisplay3D _doors3d = null;
        private DoorManager _dmanager = null;
        private MySqlConnection _connection;
        private WLD _wld;

        public DoorsDataService(MySqlConnection conn)
        {
            _connection = conn;
            _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }


        private string _zone;
        public string Zone
        {
            get { return _zone; }
        }

        public WLD WLDObject
        {
            get { return _wld; }
            set
            {
                _wld = value;
                NotifyPropertyChanged("WLDObject");
            }
        }

        public DoorManager DoorManager
        {
            get { return _dmanager; }
        }

        public void OpenModels(string file)
        {
            var s3d = S3D.Load(file);
            var wldFile = s3d.Files.FirstOrDefault(x => x.Name.Contains("_obj.wld"));
            if (wldFile == null) throw new Exception("cannot find obj wld");

            WLD wld = null;
            using(var ms = new MemoryStream(wldFile.Bytes))
            {
                wld = WLD.Load(ms);
                wld.Files = s3d;
                wld.ResolveMeshNames();
            }

            if (wld != null)
            {
                if (_doors3d != null)
                {
                    _doors3d.ObjectsWLD = wld;
                }
                WLDObject = wld;
            }
        }

        public Door GetClosestDoor(Point3D p)
        {
            if (_dmanager != null)
            {
                return _dmanager.GetClosestDoor(p);
            }
            else return null;
        }

        public void LoadZone(string zone)
        {
            _zone = zone;
            _dmanager = new DoorManager(zone, _connection, this.TypeQueryConfig);            

            if (_doors3d != null)
            {
                _doors3d.Dispose();
            }

            _doors3d = new DoorsDisplay3D(_dmanager);
            if (_viewClipping != null)
            {
                _doors3d.Clipping = _viewClipping;
            }
            _doors3d.ObjectsWLD = _wld;

            Model3D = new ModelVisual3D()
            {
                Content = _doors3d.Model,
                Transform = Transform3D
            };

            NotifyPropertyChanged("Zone");
        }

        public void ShowDoorsSelected(IEnumerable<Door> doors)
        {
            if (_doors3d != null)
            {
                _doors3d.Selected = doors;
                _doors3d.UpdateAll();
            }
        }

        private ModelVisual3D _modelVisual = null;
        public ModelVisual3D Model3D
        {
            get
            {
                return _modelVisual;
            }
            private set
            {
                _modelVisual = value;
                NotifyPropertyChanged("Model3D");
                OnModelChanged();
            }
        }

        public event Model3DChangedHandler ModelChanged;
        protected void OnModelChanged()
        {
            if (ModelChanged != null)
            {
                ModelChanged(this, new EventArgs());
            }
        }

        private Transform3D _transform3d = null;
        [Dependency("WorldTransform")]
        public Transform3D Transform3D
        {
            get { return _transform3d; }
            set
            {
                _transform3d = value;
            }
        }


        private EQEmuDisplay3D.ViewClipping _viewClipping = null;
        [Dependency]
        public EQEmuDisplay3D.ViewClipping ViewClipping
        {
            get { return _viewClipping; }
            set
            {
                _viewClipping = value;
                NotifyPropertyChanged("ViewClipping");
            }
        }
    }
}
