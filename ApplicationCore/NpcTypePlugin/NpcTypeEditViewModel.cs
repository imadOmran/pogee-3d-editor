using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using MySql.Data.MySqlClient;

using HelixToolkit.Wpf;

using ApplicationCore;
using ApplicationCore.ViewModels;
using ApplicationCore.ViewModels.Editors;

using EQEmu.Spawns;
using EQEmu.Files.S3D;
using EQEmu.Files.WLD;
using EQEmu.Database;
using EQEmuDisplay3D;

namespace NpcTypePlugin
{
    public class NpcTypeEditViewModel : ViewModelBase, IEditorViewModel
    {
        public enum ModelSource
        {
            Zone,
            Global
        }

        private int _selectedNpcTexture = 0;
        private int _selectedNpcHead = 0;

        private readonly MySqlConnection _connection;
        private readonly QueryConfig _config;
        private readonly NpcPropertyTemplateManager _templates;
        private readonly NpcAggregator _npcs;

        private INpcPropertyTemplate _selectedTemplate;
        private IEnumerable<Npc> _selectedNpcs;
        private Npc _selectedNpc;
        private string _zoneFilter;

        private WldDisplay3D _globalChrDisplay3d;
        private WldDisplay3D _zoneChrDisplay3d;

        private WLD _globalChr;
        private WLD _zoneChr;

        private ObservableCollection<object> _models = new ObservableCollection<object>();

        private DelegateCommand _applyTemplateCommand;
        private DelegateCommand _createNpcCommand;
        private DelegateCommand _removeNpcCommand;
        private DelegateCommand _copyNpcCommand;

        private DelegateCommand _incrementTextureCommand;
        private DelegateCommand _decrementTextureCommand;
        private DelegateCommand _incrementHeadCommand;
        private DelegateCommand _decrementHeadCommand;

        private Dictionary<Npc.TypeRace, string> _modelMappings;

        public NpcTypeEditViewModel(MySqlConnection connection, EQEmu.Database.QueryConfig config, NpcPropertyTemplateManager templates)
        {
            _connection = connection;
            _config = config;
            _templates = templates;

            _modelMappings = Npc.LoadModelMappings("modelmapping.xml");

            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                _npcs = new NpcAggregatorDatabase(connection, config);
            }
            else
            {
                _npcs = new NpcAggregatorLocal(config);
            }
            _npcs.Created();

            _npcs.NPCs.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(NPCs_CollectionChanged);
            Models.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Models_CollectionChanged);
        }

        void Models_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Models");
        }

        void NPCs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Npcs");
        }

        public ObservableCollection<object> Models
        {
            get { return _models; }
            private set
            {
                _models = value;
                NotifyPropertyChanged("Models");
            }
        }

        public void OpenModels(ModelSource source, string path)
        {
            var s3d = S3D.Load(path);
            var wld = s3d.Files.FirstOrDefault(x => x.Name.Contains(".wld"));
            if (wld != null)
            {
                if (source == ModelSource.Global)
                {
                    using(var ms = new MemoryStream(wld.Bytes))
                    {
                        Models.Clear();

                        _globalChr = WLD.Load(ms);
                        _globalChr.Files = s3d;
                        _globalChrDisplay3d = new WldDisplay3D(_globalChr);
                        _globalChrDisplay3d.UpdateAll();

                        System.Threading.ThreadPool.QueueUserWorkItem(x =>
                        {
                            _globalChr.ResolveMeshNames();
                        });

                        Models.Add(new DefaultLights());
                        Models.Add(new CoordinateSystemVisual3D());
                        Models.Add(
                            new ModelVisual3D()
                            {
                                Content= _globalChrDisplay3d.Model
                            });
                    }
                }
                else if (source == ModelSource.Zone)
                {
                    using (var ms = new MemoryStream(wld.Bytes))
                    {
                        Models.Clear();

                        _zoneChr = WLD.Load(ms);
                        _zoneChr.Files = s3d;
                        _zoneChrDisplay3d = new WldDisplay3D(_zoneChr);
                        _zoneChrDisplay3d.UpdateAll();

                        System.Threading.ThreadPool.QueueUserWorkItem(x =>
                        {
                            _zoneChr.ResolveMeshNames();
                        });

                        Models.Add(new DefaultLights());
                        Models.Add(new CoordinateSystemVisual3D());
                        Models.Add(
                            new ModelVisual3D()
                            {
                                Content = _zoneChrDisplay3d.Model
                            });
                    }
                }
            }
        }

        public void OpenXML(string file)
        {
            _npcs.ClearCache();
            _npcs.LoadXML(file);
            _npcs.LoadCached();
            NotifyPropertyChanged("Npcs");
        }

        public void SaveXML(string directory)
        {
            _npcs.SaveXML(directory);
        }

        public void User3DClickAt(object sender, World3DClickEventArgs e)
        {
        }

        public ApplicationCore.DataServices.IDataService Service
        {
            get { throw new NotImplementedException(); }
        }

        public NpcAggregator NpcManager
        {
            get { return _npcs; }
        }

        public NpcPropertyTemplateManager NpcTemplates
        {
            get { return _templates; }
        }
        
        public INpcPropertyTemplate SelectedTemplate
        {
            get { return _selectedTemplate; }
            set
            {
                _selectedTemplate = value;
                NotifyPropertyChanged("SelectedTemplate");
                ApplyTemplateCommand.RaiseCanExecuteChanged();
            }
        }

        public IEnumerable<Npc> Npcs
        {
            get { return _npcs.NPCs; }
        }

        public int SelectedNpcTexture
        {
            get { return _selectedNpcTexture; }
            set
            {
                _selectedNpcTexture = value;
                SelectedNpc.Texture = value;

                RenderSelectedNpc();
                NotifyPropertyChanged("SelectedNpcTexture");
            }
        }

        public int SelectedNpcHead
        {
            get { return _selectedNpcHead; }
            set
            {
                _selectedNpcHead = value;
                SelectedNpc.HelmTexture = value;

                RenderSelectedNpc();
                NotifyPropertyChanged("SelectedNpcHead");
            }
        }
        
        public IEnumerable<Npc> SelectedNpcs
        {
            get { return _selectedNpcs; }
            set
            {
                _selectedNpcs = value;
                if (_selectedNpcs.Count() > 0)
                {
                    SelectedNpc = _selectedNpcs.First();
                }
                else { SelectedNpc = null; }

                NotifyPropertyChanged("SelectedNpcs");
            }
        }

        private void RenderSelectedNpc()
        {
            bool exit = false;

            if (_selectedNpc == null) exit = true;
            if (!exit)
            {
                if (_modelMappings.ContainsKey(_selectedNpc.Race))
                {
                    var modelStr = _modelMappings[_selectedNpc.Race];
                    if (modelStr.Contains('#'))
                    {
                        if (_selectedNpc.Gender == Npc.TypeGender.Female)
                        {
                            modelStr = modelStr.Replace('#', 'F');
                        }
                        else
                        {
                            modelStr = modelStr.Replace('#', 'M');
                        }
                    }

                    if (_zoneChrDisplay3d != null)
                    {
                        _zoneChrDisplay3d.RenderModel(modelStr, _selectedNpc.Texture, _selectedNpc.HelmTexture);
                    }

                    if (_globalChrDisplay3d != null)
                    {
                        _globalChrDisplay3d.RenderModel(modelStr, _selectedNpc.Texture, _selectedNpc.HelmTexture);
                    }
                }
            }

            IncrementHeadCommand.RaiseCanExecuteChanged();
            DecrementHeadCommand.RaiseCanExecuteChanged();
            IncrementTextureCommand.RaiseCanExecuteChanged();
            DecrementTextureCommand.RaiseCanExecuteChanged();
        }
        
        public Npc SelectedNpc
        {
            get
            {
                return _selectedNpc;
            }
            set
            {
                _selectedNpc = value;

                if (_selectedNpc != null)
                {
                    SelectedNpcHead = _selectedNpc.HelmTexture;
                    SelectedNpcTexture = _selectedNpc.Texture;
                }

                if (_selectedNpcs == null) _selectedNpcs = new List<Npc>() { value };
                RenderSelectedNpc();

                //if (_selectedNpc != null)
                //{
                //    if (_modelMappings.ContainsKey(_selectedNpc.Race))
                //    {
                //        var modelStr = _modelMappings[_selectedNpc.Race];
                //        if (modelStr.Contains('#'))
                //        {
                //            if (_selectedNpc.Gender == Npc.TypeGender.Female)
                //            {
                //                modelStr = modelStr.Replace('#', 'F');
                //            }
                //            else
                //            {
                //                modelStr = modelStr.Replace('#', 'M');
                //            }
                //        }

                //        if (_zoneChrDisplay3d != null)
                //        {
                //            _zoneChrDisplay3d.RenderModel(modelStr, _selectedNpc.Texture, _selectedNpc.HelmTexture);
                //        }

                //        if (_globalChrDisplay3d != null)
                //        {
                //            _globalChrDisplay3d.RenderModel(modelStr, _selectedNpc.Texture, _selectedNpc.HelmTexture);
                //        }
                //    }
                //}

                NotifyPropertyChanged("SelectedNpc");
                ApplyTemplateCommand.RaiseCanExecuteChanged();
                RemoveNpcCommand.RaiseCanExecuteChanged();
                CreateNpcCommand.RaiseCanExecuteChanged();
                CopyNpcCommand.RaiseCanExecuteChanged();
            }
        }

        public string ZoneFilter
        {
            get
            {
                return _zoneFilter;
            }
            set
            {
                if (value != null)
                {
                    _zoneFilter = value;
                    _npcs.LookupByZone(_zoneFilter);                    
                }
                
                NotifyPropertyChanged("ZoneFilter");
                NotifyPropertyChanged("Npcs");
            }
        }

        public DelegateCommand IncrementTextureCommand
        {
            get
            {
                if (_incrementTextureCommand == null)
                {
                    _incrementTextureCommand = new DelegateCommand(
                        x =>
                        {
                            SelectedNpcTexture += 1;
                        },
                        y =>
                        {
                            return SelectedNpc != null;
                        });
                }
                return _incrementTextureCommand;
            }
        }

        public DelegateCommand DecrementTextureCommand
        {
            get
            {
                if (_decrementTextureCommand == null)
                {
                    _decrementTextureCommand = new DelegateCommand(
                        x =>
                        {
                            SelectedNpcTexture -= 1;
                        },
                        y =>
                        {
                            return SelectedNpc != null && SelectedNpcTexture > 0;
                        });
                }
                return _decrementTextureCommand;
            }
        }

        public DelegateCommand IncrementHeadCommand
        {
            get
            {
                if (_incrementHeadCommand == null)
                {
                    _incrementHeadCommand = new DelegateCommand(
                        x =>
                        {
                            SelectedNpcHead += 1;
                        },
                        y =>
                        {
                            return SelectedNpc != null;
                        });
                }
                return _incrementHeadCommand;
            }
        }

        public DelegateCommand DecrementHeadCommand
        {
            get
            {
                if (_decrementHeadCommand == null)
                {
                    _decrementHeadCommand = new DelegateCommand(
                        x =>
                        {
                            SelectedNpcHead -= 1;
                        },
                        y =>
                        {
                            return SelectedNpc != null && SelectedNpcHead > 0;
                        });
                }
                return _decrementHeadCommand;
            }
        }

        public DelegateCommand ApplyTemplateCommand
        {
            get
            {
                if (_applyTemplateCommand == null)
                {
                    _applyTemplateCommand = new DelegateCommand(
                        x =>
                        {
                            SelectedTemplate.SetProperties(SelectedNpcs);
                            OnTemplateAppliedToNpc(SelectedNpcs, SelectedTemplate);
                        },
                        y =>
                        {
                            return SelectedNpc != null && SelectedTemplate != null;
                        });
                }

                return _applyTemplateCommand;
            }
        }

        public DelegateCommand CreateNpcCommand
        {
            get
            {
                if (_createNpcCommand == null)
                {
                    _createNpcCommand = new DelegateCommand(
                        x =>
                        {
                            var npc = _npcs.CreateNPC();
                            npc.Created();
                            _npcs.AddNPC(npc);
                            SelectedNpc = npc;
                        },
                        y =>
                        {
                            return _npcs != null;
                        });
                }
                return _createNpcCommand;
            }
        }

        public DelegateCommand RemoveNpcCommand
        {
            get
            {
                if (_removeNpcCommand == null)
                {
                    _removeNpcCommand = new DelegateCommand(
                        x =>
                        {
                            if (SelectedNpcs != null)
                            {
                                foreach (var npc in SelectedNpcs.ToArray())
                                {
                                    _npcs.RemoveNPC(npc);
                                }
                                SelectedNpc = null;
                            }
                            else
                            {
                                _npcs.RemoveNPC(SelectedNpc);
                                SelectedNpc = null;
                            }
                            NotifyPropertyChanged("Npcs");
                        },
                        y =>
                        {
                            return _npcs != null && SelectedNpc != null;
                        });
                }
                return _removeNpcCommand;
            }
        }

        public DelegateCommand CopyNpcCommand
        {
            get
            {
                if (_copyNpcCommand == null)
                {
                    _copyNpcCommand = new DelegateCommand(
                        x =>
                        {
                            if (SelectedNpcs != null)
                            {
                                foreach (var npc in SelectedNpcs)
                                {
                                    _npcs.AddNPC(npc.ShallowCopy());                                    
                                }
                            }
                            else
                            {
                                _npcs.AddNPC(SelectedNpc.ShallowCopy());
                            }

                        },
                        y =>
                        {
                            return SelectedNpc != null;
                        });
                }
                return _copyNpcCommand;
            }
        }

        public event TemplateApplied TemplateAppliedToNpc;

        private void OnTemplateAppliedToNpc(IEnumerable<Npc> npcs, INpcPropertyTemplate template)
        {
            var e = TemplateAppliedToNpc;
            if (e != null)
            {
                e(this, new TemplateAppliedEventArgs(template, npcs));
            }
        }
    }

    public delegate void TemplateApplied(object sender, TemplateAppliedEventArgs e);

    public class TemplateAppliedEventArgs
    {
        public TemplateAppliedEventArgs(INpcPropertyTemplate template, IEnumerable<Npc> npcs)
        {
            Template = template;
            AffectedNpcs = npcs;
        }

        public IEnumerable<Npc> AffectedNpcs
        {
            get;
            private set;
        }

        public INpcPropertyTemplate Template
        {
            get;
            private set;
        }
    }
}
