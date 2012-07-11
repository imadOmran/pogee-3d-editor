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

        private DelegateCommand _applyTemplateCommand;

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
