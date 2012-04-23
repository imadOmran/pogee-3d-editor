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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using System.CodeDom.Compiler;

using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Practices.Unity;
using Unity.AutoRegistration;

using MySql.Data.MySqlClient;

using HelixToolkit;
using HelixToolkit.Wpf;

using ApplicationCore.ViewModels.Editors;

namespace ApplicationCore
{
    public enum ClippingParameter
    {
        MaxX, MinX, MaxY, MinY, MaxZ, MinZ
    };

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private IUnityContainer _container = new UnityContainer();
        public IUnityContainer Container
        {
            get { return _container; }
        }

        private ObservableCollection<UserControls.Ribbon.IRibbonItem> _ribbonItems =
            new ObservableCollection<UserControls.Ribbon.IRibbonItem>();
        public ObservableCollection<UserControls.Ribbon.IRibbonItem> RibbonItems
        {
            get { return _ribbonItems; }
            private set
            {
                _ribbonItems = value;
                NotifyPropertyChanged("RibbonItems");
            }
        }
        
        public MainWindowViewModel()
        {
            Registration();
            CreateRibbonTabs();
            SubscribeTo3DChanges();
            SubscribeToSelectionChanges();
        }

        private void CreateRibbonTabs()
        {
            var tabs = _container.ResolveAll<UserControls.Ribbon.Tabs.IRibbonTab>();
            
            foreach (var tab in tabs.OrderBy( x => x.ToString() ) )
            {
                RibbonItems.Add(tab);
            }
        }

        public MySqlConnection DBConnection
        {
            get { return _container.Resolve<MySqlConnection>(); }
        } 

        private void Registration()
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                return;
            }

            #region register database connection configuration
            EQEmu.Database.Configuration conf = new EQEmu.Database.Configuration();
            MySqlConnection connection = GetDatabaseConnection(ref conf);
            if (connection != null)
            {
                _container.RegisterInstance<MySqlConnection>(connection);
            }
            else
            {
                _container.RegisterInstance<MySqlConnection>(new MySqlConnection());
                System.Windows.MessageBox.Show("Could not create database connection, configure connection.xml and restart");
            }

            _container.RegisterInstance<EQEmu.Database.Configuration>(conf);
            #endregion

            #region register the type to query mappings

            XDocument doc;
            EQEmu.Database.QueryConfig config = null;

            if (!DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
#if EQEMU
                doc = XDocument.Load("eqemu-config.xml");
#else
                doc = XDocument.Load("config.xml");
#endif
                config = EQEmu.Database.QueryConfig.Create(doc.Root);
                _container.RegisterInstance<EQEmu.Database.QueryConfig>(config);
            }            

            #endregion

            #region register 3D 'things' transforms / cameras / etc

            _container.RegisterInstance<ProjectionCamera>(new PerspectiveCamera()
            {
                Position = new Point3D(0, 0, 0),
                FieldOfView = 45,
                UpDirection = new Vector3D(0, 0, 1),
                LookDirection = new Vector3D(0, 0, 0)
            });

            _container.RegisterInstance<Transform3D>("WorldTransform", new ScaleTransform3D()
            {
                ScaleX = -1
            });

            _container.RegisterInstance<EQEmuDisplay3D.ViewClipping>(_viewClipping);

            #endregion

            //loads the assemblies in the Plugins directory
            DirectoryCatalog dirCatalog;
            if (!DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                dirCatalog = new DirectoryCatalog(".\\Plugins");
            }

            //this will create a new and load a new assembly, the unity container will be able to resolve types from here
            CompilePluginsDirectory();

            _container.ConfigureAutoRegistration()
                .ExcludeSystemAssemblies()

                .Include(
                    x => x.DecoratedWith<AutoRegisterAttribute>() && x.Implements<UserControls.Ribbon.Tabs.IRibbonTab>(),
                    Then.Register().WithTypeName().UsingSingletonMode()
                    )

                .Include(
                    x => x.DecoratedWith<AutoRegisterAttribute>() && x.Implements<DataServices.IDataService>(),
                    //Then.Register().WithTypeName().AsAllInterfacesOfType().UsingSingletonMode()
                    Then.Register().WithTypeName().UsingSingletonMode()
                    )

                .Include(
                    x => x.DecoratedWith<AutoRegisterAttribute>() && x.Implements<UserControls.ITabEditorControl>(),
                    Then.Register().WithTypeName().UsingSingletonMode()
                    )

                .Include(
                    x => x.Implements<EQEmu.Spawns.INPCPropertyTemplate>(),
                    Then.Register().WithTypeName().UsingSingletonMode()
                    )
                    

                .ApplyAutoRegistration();

            //we need to eventually make this more generic so we don't end up with a ton of these
            var npcmanager = new EQEmu.Spawns.NPCPropertyTemplateManager();
            npcmanager.Templates = _container.ResolveAll<EQEmu.Spawns.INPCPropertyTemplate>();
            _container.RegisterInstance(npcmanager);
        }

        private void CompilePluginsDirectory()
        {
            var compiler = new Microsoft.CSharp.CSharpCodeProvider();
            var parms = new CompilerParameters()
            {
                GenerateExecutable = false,
                GenerateInMemory = true
            };
            parms.ReferencedAssemblies.Add("System.Core.dll");
            parms.ReferencedAssemblies.Add("EQEmu.dll");

            var files = Directory.GetFiles(".\\Plugins");
            if (files.Count() > 0)
            {
                files = files.Where(x => x.Contains(".cs")).ToArray();
                if (files.Count() > 0)
                {
                    var results = compiler.CompileAssemblyFromFile(parms, files);
                    if (results.Errors.HasErrors)
                    {
                        string errorMessage = "There were compilation errors from source files in the plugin directory:" + System.Environment.NewLine; ;
                        foreach (var err in results.Errors)
                        {
                            errorMessage += err.ToString() + System.Environment.NewLine;
                        }

                        var window = new TextWindow(errorMessage);
                        window.ShowDialog();
                    }
                    else
                    {
                        //the get accessor loads the assembly into the appdomain
                        var assembly = results.CompiledAssembly;
                    }
                }
            }
            
        }

        public void WorldMouseClickAt(Point3D p,object selectedControl, IsPointInsideSelectionBox selectionBoxCheck )
        {
            foreach (var control in Container.ResolveAll<UserControls.IEditorControl>())
            {
                if (control == null || control.ViewModel == null) continue;
                try
                {
                    control.ViewModel.User3DClickAt(
                        this,
                        new ViewModels.Editors.World3DClickEventArgs()
                        {
                            ActiveRibbonControl = selectedControl as UserControls.Ribbon.IRibbonItem,
                            PointInWorld = p,
                            CheckSelection = selectionBoxCheck
                        });
                }
                catch (System.Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message, control.ToString());
                }
            }
        }

        private Dictionary<DataServices.IModel3DProvider, ModelVisual3D> _modelMapping =
            new Dictionary<DataServices.IModel3DProvider, ModelVisual3D>();

        public IEnumerable<UserControls.ITabEditorControl> ResolveTabEditorControls()
        {
            return _container.ResolveAll<UserControls.ITabEditorControl>();
        }

        private void SubscribeTo3DChanges()
        {
            foreach (var model in _container.ResolveAll<DataServices.IDataService>())
            {
                DataServices.IModel3DProvider modelProvider = model as DataServices.IModel3DProvider;
                if (modelProvider != null)
                {
                    modelProvider.ModelChanged += new DataServices.Model3DChangedHandler(model_ModelChanged);
                }
            }
        }

        private object _selectedObject = null;
        public object SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                _selectedObject = value;
                NotifyPropertyChanged("SelectedObject");
            }
        }

        private void SubscribeToSelectionChanges()
        {
            foreach (var ctrl in _container.ResolveAll<UserControls.IEditorControl>())
            {
                ctrl.ObjectSelected += new UserControls.ObjectSelected(ctrl_ObjectSelected);
            }
        }

        void ctrl_ObjectSelected(object sender, UserControls.ObjectSelectedEventArgs args)
        {
            SelectedObject = args.Object;
        }

        private ObservableCollection<object> _models =
            new ObservableCollection<object>();
        public ObservableCollection<object> Models
        {
            get { return _models; }
            private set
            {
                _models = value;
                NotifyPropertyChanged("Models");
            }
        }

        void model_ModelChanged(object sender, System.EventArgs e)
        {
            if (Models.Count == 0)
            {
                Models.Add(new DefaultLights());
                Models.Add(new CoordinateSystemVisual3D());
            }

            var obj = sender as DataServices.IModel3DProvider;
            if (obj != null && obj.Model3D != null)
            {
                if (_modelMapping.ContainsKey(obj))
                {
                    Models.Remove(_modelMapping[obj]);
                }                    

                Models.Add(obj.Model3D);
                _modelMapping[obj] = obj.Model3D;
            }
            NotifyPropertyChanged("Models");
        }

        private EQEmuDisplay3D.ViewClipping _viewClipping = new EQEmuDisplay3D.ViewClipping();
        public EQEmuDisplay3D.ViewClipping ViewClipping
        {
            get { return _viewClipping; }
            set
            {
                _viewClipping = value;
                NotifyPropertyChanged("ViewClipping");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private MySqlConnection GetDatabaseConnection(ref EQEmu.Database.Configuration configuration)
        {
            XmlSerializer serializer =
                new XmlSerializer(typeof(EQEmu.Database.Configuration));
            MySql.Data.MySqlClient.MySqlConnection conn = null;

            if (!File.Exists("./connection.xml")) return null;
            EQEmu.Database.Configuration conf;

            using (XmlReader reader = XmlReader.Create("./connection.xml"))
            {
                conf = (EQEmu.Database.Configuration)serializer.Deserialize(reader);
                string connStr = string.Format("server={0};user={1};database={2};port={3};password={4};ConnectionTimeout={5};",
                    conf.Host, conf.User, conf.Database, conf.Port, conf.Password, conf.ConnectionTimeout);
                conn = new MySql.Data.MySqlClient.MySqlConnection(connStr);
            }

            if (configuration != null && conf != null)
            {
                configuration = conf;
            }

            return conn;
        }
    }
}
