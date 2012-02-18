using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using MySql.Data.MySqlClient;

namespace EQEmu.Spawns
{
    public class NPCAggregator : Database.ManageDatabase
    {
        private ObservableCollection<NPC> _npcs = new ObservableCollection<NPC>();
        private readonly MySqlConnection _connection;

        public NPCAggregator(MySqlConnection connection, Database.QueryConfig config)
            : base(config)
        {
            if (connection == null) throw new Exception("must provide database connection");
            _connection = connection;
        }

        public ObservableCollection<NPC> NPCs
        {
            get { return _npcs; }
        }

        //this is kind of a pointless property... only used for reflection property setting in query
        public string FilterName
        {
            get;
            set;
        }
        
        public void Lookup(string name)
        {
            FilterName = name;
            var sql = String.Format(Queries.SelectQuery,ResolveArgs(Queries.SelectArgs));            
            var results = Database.QueryHelper.RunQuery(_connection,sql);

            _npcs.Clear();

            foreach (var dictionary in results)
            {
                var npc = new NPC(_queryConfig);
                npc.SetProperties(Queries,dictionary);
                _npcs.Add(npc);
            }
        }

        public override List<Database.IDatabaseObject> DirtyComponents
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class NPC : Database.DatabaseObject
    {
        public NPC(Database.QueryConfig conf)
            : base(conf)
        {

        }

        public string Name
        {
            get;
            set;
        }

        public int Level
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }
    }
}
