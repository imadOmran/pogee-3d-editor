using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;

using Microsoft.Practices.Unity;

using ApplicationCore;
using ApplicationCore.DataServices;

using EQEmu.Files;
using EQEmu.Files.WLD;
using EQEmu.Files.WLD.Fragments;
using EQEmu.Files.S3D;

namespace S3DPlugin
{
    public class LoadStatus
    {
        public int PercentDone
        {
            get;
            set;
        }

        public string OperationDescription
        {
            get;
            set;
        }
    }

    [AutoRegister]
    public class S3DDataService : DataServiceBase, IModel3DProvider
    {
        public S3DDataService()
            : base()
        {
            // this is bad.... this should not be used like a viewmodel... rewrite is in order
            _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            Status.OperationDescription = "Idle";
            Status.PercentDone = 0;
        }

        private System.Windows.Threading.Dispatcher _dispatcher = null;
        private EQEmuDisplay3D.WldDisplay3D _display3d = null;
        private WLD _wld = null;
        private WLD _objectLocations = null;
        private WLD _objects = null;

        private IEnumerable<WLD> _wlds = null;

        public IEnumerable<WLD> WLDCollection
        {
            get { return _wlds; }
        }

        private LoadStatus _status = new LoadStatus();
        public LoadStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }
        
        public WLD WLDObject
        {
            get { return _wld; }
            set
            {
                _wld = value;

                if (_display3d != null)
                {
                    _display3d.Dispose();
                }

                _display3d = new EQEmuDisplay3D.WldDisplay3D(_wld, _objectLocations, _objects);

                if(_viewClipping != null)
                {
                    _display3d.Clipping = _viewClipping;
                }

                Model3D = new ModelVisual3D()
                {
                    Content = _display3d.Model,          
                    //Transform = Transform3D                   
                };

                NotifyPropertyChanged("WLDObject");
                NotifyPropertyChanged("WLDCollection");
            }
        }

        public void RenderMesh(EQEmu.Files.WLD.Fragments.Mesh mesh)
        {
            if (mesh == null) return;
            _display3d.RenderMesh(new List<Mesh>() { mesh });
        }

        public void RenderAll()
        {
            if (_display3d != null)
            {
                _display3d.UpdateAll();
            }
        }

        private void DoStatusUpdate(int percentDone, string description,bool async=true)
        {
            var action = new Action(() =>
                {
                    Status = new LoadStatus()
                    {
                        PercentDone = percentDone,
                        OperationDescription = description
                    };
                });

            if (async)
            {
                _dispatcher.BeginInvoke(action);
            }
            else
            {
                _dispatcher.Invoke(action);
            }
        }
        
        public void OpenFile(string file,bool getObjects=false)
        {
            if (File.Exists(file))
            {
                Task.Factory.StartNew( () =>
                    {
                        S3D s3d = null;
                        int status = 0;

                        status += 5;
                        DoStatusUpdate(status, "Loading " + file);

                        try
                        {
                            s3d = S3D.Load(file);
                        }
                        catch (Exception)
                        {
                            return;
                        }

                        var filename = System.IO.Path.GetFileName(file);
                        int period = filename.IndexOf('.', 0);
                        var zone = filename.Substring(0, period);
                        string dir = Path.GetDirectoryName(file);

                        var archiveFile = s3d.Files.FirstOrDefault(x => x.Name.Contains(zone + ".wld"));
                        if (archiveFile == null) return;

                        _wld = null;
                        _objectLocations = null;
                        List<WLD> wlds = new List<WLD>();

                        foreach(var archive in s3d.Files.Where( x => x.Name.Contains(".wld") ) )
                        {
                            bool isObjects = archive.Name.Contains("objects.wld");
                            if (isObjects && !getObjects) continue;
                            
                            status += 20;
                            DoStatusUpdate(status, "Loading " + archive.Name);

                            using (var ms = new MemoryStream(archive.Bytes))
                            {
                                WLD wld = null;
                                try
                                {
                                    wld = WLD.Load(ms);
                                }
                                catch (Exception e)
                                {
                                    DoStatusUpdate(0, "Failed loading: " + archive.Name + " " + e.Message, false);
                                    return;
                                }
                                wlds.Add(wld);
                                if(archive.Name.Contains( zone + ".wld" ) )
                                {
                                    _wld = wld;
                                    if (archive.Name.Contains("_obj"))
                                    {
                                        _wld.ResolveMeshNames();
                                    }
                                }
                                else if (isObjects)
                                {
                                    _objectLocations = wld;
                                    if (_objectLocations != null)
                                    {
                                        _objectLocations.ResolveObjectLocationNames();
                                    }
                                }
                            }
                        }
                        _wlds = wlds;

                        //load up the _obj.s3d WLD                        
                        string objFile = dir+"\\"+zone+"_obj.s3d";                       
                        
                        if (getObjects && File.Exists(objFile))
                        {
                            status += 10;
                            DoStatusUpdate(status, "Loading " + zone + "_obj.s3d");
                            var objS3d = S3D.Load(objFile);
                            var archive = objS3d.Files.FirstOrDefault(x => x.Name.Contains(".wld"));
                            if (archive != null)
                            {
                                using (var ms = new MemoryStream(archive.Bytes))
                                {
                                    WLD wld = null;
                                    try
                                    {
                                        wld = WLD.Load(ms);
                                    }
                                    catch (Exception e)
                                    {
                                        DoStatusUpdate(0, "Failed loading: " + archive.Name + " " + e.Message, false);
                                        return;
                                    }                                    
                                    wlds.Add(wld);
                                    _objects = wld;
                                    _objects.Files = objS3d;
                                }                                
                                if (_objects != null)
                                {
                                    _objects.ResolveMeshNames();
                                }
                            }
                        }
                         

                        if (wlds.Count > 0)
                        {
                            status = 90;
                            DoStatusUpdate(status, "Generating Geometry",false);

                            _wld = _wld == null ? wlds.ElementAt(0) : _wld;

                            _dispatcher.BeginInvoke((Action)(() =>
                                {
                                    _wld.Files = s3d;
                                    WLDObject = _wld;
                                    _wlds = wlds;

                                    Status = new LoadStatus()
                                    {
                                        PercentDone = 100,
                                        OperationDescription = "Done"
                                    };
                                }));
                        }
                    });
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
