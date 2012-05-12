using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

using MySql.Data.MySqlClient;

namespace EQEmu.Spawns
{
    public class SpawnEntry : Database.DatabaseObject
    {
        private int _spawnGroupId;
        private int _npcId;
        private short _chance;
        private NPC _npc;

        private SpawnEntry()
            : base(null)
        {

        }

        public SpawnEntry(Database.QueryConfig config)
            : base(config)
        {

        }

        public int SpawnGroupID
        {
            get { return _spawnGroupId; }
            set
            {
                _spawnGroupId = value;
                Dirtied();
            }
        }

        [Browsable(false)]
        [XmlIgnore]
        public NPC NPC
        {
            get { return _npc; }
            set
            {
                _npc = value;
                NpcID = value.Id;
            }
        }

        public int NpcID
        {
            get
            {
                if (_npc == null)
                {
                    return _npcId;
                }
                else
                {
                    return _npc.Id;
                }
            }
            set
            {
                _npcId = value;
                Dirtied();
            }
        }

        public short Chance
        {
            get { return _chance; }
            set
            {
                _chance = value;
                Dirtied();
            }
        }

        [XmlIgnore]
        public string NpcName
        {
            get;
            set;
        }

        [XmlIgnore]
        public short NpcLevel
        {
            get;
            set;
        }
    }
}
