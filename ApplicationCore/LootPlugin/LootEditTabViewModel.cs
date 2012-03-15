﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EQEmu.Loot;
using EQEmu.Spawns;

using ApplicationCore;

namespace LootPlugin
{
    public class LootEditTabViewModel : ApplicationCore.ViewModels.ViewModelBase
    {
        private LootTableAggregator _manager;
        private NPCAggregator _npcmanager;

        private DelegateCommand _clearCacheCommand;
        private DelegateCommand _createLootTableCommand;
        private DelegateCommand _createLootDropCommand;

        public LootEditTabViewModel(LootTableAggregator manager,NPCAggregator npcmanager)
        {
            _manager = manager;
            _manager.Created();
            _npcmanager = npcmanager;

            _addExistingLootDropCommand = new DelegateCommand(
                x =>
                {
                    var lootdrop = _manager.CreateLootDrop();
                    lootdrop.Id = -1;
                    lootdrop.Lookup(Int32.Parse(x.ToString()));
                    if (lootdrop.Id >= 0)
                    {
                        lootdrop.LootTableId = SelectedLootTable.Id;
                        SelectedLootTable.AddLootDrop(lootdrop);
                    }
                },
                x =>
                {
                    return SelectedLootTable != null;
                });

            ClearCacheCommand = new DelegateCommand(
                x =>
                {
                    _manager.ClearCache();
                },
                x =>
                {
                    return true;
                });

            CreateLootTableCommand = new DelegateCommand(
                x =>
                {
                    var table = _manager.CreateLootTable(true);
                    if (table != null)
                    {
                        _manager.AddLootTable(table);
                    }
                },
                x =>
                {
                    return true;
                });

            CreateLootDropCommand = new DelegateCommand(
                x =>
                {
                    var drop = _manager.CreateLootDrop(true);
                    if (drop != null)
                    {
                        SelectedLootTable.AddLootDrop(drop);
                    }

                },
                x =>
                {
                    return SelectedLootTable != null;
                });

        }

        public string GetSQL()
        {
            return _manager.GetSQL();
        }

        private int _filterId;
        public int FilterId
        {
            get { return _filterId; }
            set
            {
                _filterId = value;
                _manager.Lookup(value);
                NotifyPropertyChanged("FilterId");
            }
        }

        private string _npcLookup;
        public string NPCLookup
        {
            get { return _npcLookup; }
            set
            {
                _npcLookup = value;
                _npcmanager.Lookup(value);
                NPCItems = _npcmanager.NPCs;
                NotifyPropertyChanged("NPCLookup");
            }
        }

        private IEnumerable<NPC> _npcItems;
        public IEnumerable<NPC> NPCItems
        {
            get { return _npcItems; }
            set
            {
                _npcItems = value;
                NotifyPropertyChanged("NPCItems");
            }
        }

        private string _tableLookup;
        public string TableLookup
        {
            get { return _tableLookup; }
            set
            {
                _tableLookup = value;
                TablesLookedUp = _manager.LookupTables(value);
                NotifyPropertyChanged("TableLookup");
            }
        }

        private IEnumerable<object> _tablesLookedUp;
        public IEnumerable<object> TablesLookedUp
        {
            get { return _tablesLookedUp; }
            set
            {
                _tablesLookedUp = value;
                NotifyPropertyChanged("TablesLookedUp");
            }
        }

        private string _itemNameLookup;
        public string ItemNameLookup
        {
            get { return _itemNameLookup; }
            set
            {
                _itemNameLookup = value;
                ItemsLookedUp = _manager.LookupItems(value);
                NotifyPropertyChanged("ItemNameLookup");
            }
        }
        
        private IEnumerable<Item> _itemsLookedUp;
        public IEnumerable<Item> ItemsLookedUp
        {
            get { return _itemsLookedUp; }
            set
            {
                _itemsLookedUp = value;
                NotifyPropertyChanged("ItemsLookedUp");
            }
        }

        private IEnumerable<LootDrop> _dropsLookedUp;
        public IEnumerable<LootDrop> DropsLookedUp
        {
            get { return _dropsLookedUp; }
            set
            {
                _dropsLookedUp = value;                
                NotifyPropertyChanged("DropsLookedUp");
            }
        }

        private LootTable _selectedLootTable;
        public LootTable SelectedLootTable
        {
            get { return _selectedLootTable; }
            set
            {
                _selectedLootTable = value;
                NotifyPropertyChanged("SelectedLootTable");
                AddExistingLootDropCommand.RaiseCanExecuteChanged();
                CreateLootDropCommand.RaiseCanExecuteChanged();
            }
        }

        private LootDrop _selectedLootDrop;
        public LootDrop SelectedLootDrop
        {
            get { return _selectedLootDrop; }
            set
            {
                _selectedLootDrop = value;
                NotifyPropertyChanged("SelectedLootDrop");
            }
        }

        private LootDropEntry _selectedDropEntry;
        public LootDropEntry SelectedDropEntry
        {
            get { return _selectedDropEntry; }
            set 
            {
                _selectedDropEntry = value;
                NotifyPropertyChanged("SelectedDropEntry");
            }
        }

        private DelegateCommand _addExistingLootDropCommand;
        public DelegateCommand AddExistingLootDropCommand
        {
            get { return _addExistingLootDropCommand; }
            set
            {
                _addExistingLootDropCommand = value;
                NotifyPropertyChanged("AddExistingLootDropCommand");
            }
        }

        public DelegateCommand ClearCacheCommand
        {
            get { return _clearCacheCommand; }
            set
            {
                _clearCacheCommand = value;
                NotifyPropertyChanged("ClearCacheCommand");
            }
        }

        public DelegateCommand CreateLootTableCommand
        {
            get { return _createLootTableCommand; }
            set
            {
                _createLootTableCommand = value;
                NotifyPropertyChanged("CreateLootTableCommand");
            }
        }

        public DelegateCommand CreateLootDropCommand
        {
            get { return _createLootDropCommand; }
            set
            {
                _createLootDropCommand = value;
                NotifyPropertyChanged("CreateLootDropCommand");
            }
        }
               
        public IEnumerable<LootTable> Cache
        {
            get { return _manager.LootTables; }
        }
    }
}
