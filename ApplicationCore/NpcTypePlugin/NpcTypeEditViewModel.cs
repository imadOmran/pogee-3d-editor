using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;

using ApplicationCore;
using ApplicationCore.ViewModels;
using ApplicationCore.ViewModels.Editors;

using EQEmu.Spawns;
using EQEmu.Database;

namespace NpcTypePlugin
{
    public class NpcTypeEditViewModel : ViewModelBase, IEditorViewModel
    {
        private readonly MySqlConnection _connection;
        private readonly QueryConfig _config;
        private readonly NpcPropertyTemplateManager _templates;
        private readonly NpcAggregator _npcs;

        private INpcPropertyTemplate _selectedTemplate;
        private IEnumerable<Npc> _selectedNpcs;
        private Npc _selectedNpc;
        private string _zoneFilter;

        private DelegateCommand _applyTemplateCommand;
        private DelegateCommand _createNpcCommand;
        private DelegateCommand _removeNpcCommand;
        private DelegateCommand _copyNpcCommand;

        public NpcTypeEditViewModel(MySqlConnection connection, EQEmu.Database.QueryConfig config, NpcPropertyTemplateManager templates)
        {
            _connection = connection;
            _config = config;
            _templates = templates;

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
        }

        void NPCs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Npcs");
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
        
        public Npc SelectedNpc
        {
            get
            {
                return _selectedNpc;
            }
            set
            {
                _selectedNpc = value;
                if (_selectedNpcs == null) _selectedNpcs = new List<Npc>() { value };
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
